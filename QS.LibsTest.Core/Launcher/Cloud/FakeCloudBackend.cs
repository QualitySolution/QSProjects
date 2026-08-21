using Grpc.Core;
using QS.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QS.Launcher.Test.Cloud {
	public sealed class FakeCloudBackend : IDisposable {
		private readonly Server server;

		public int Port { get; }

		/// <summary>Состояние облака: аккаунт, его пользователи, базы и доступы</summary>
		public CloudState State { get; } = new CloudState();

		public FakeCloudBackend() {
			server = new Server {
				Services = {
					LoginManagement.BindService(new LoginService(State)),
					DataBaseManagement.BindService(new DataBaseService(State)),
					UserManagement.BindService(new UserService(State)),
					SessionManagement.BindService(new SessionService())
				},
				Ports = { new ServerPort("127.0.0.1", ServerPort.PickUnused, ServerCredentials.Insecure) }
			};
			server.Start();
			Port = server.Ports.First().BoundPort;
		}

		public void Dispose() => server.ShutdownAsync().GetAwaiter().GetResult();

		#region Состояние

		public sealed class CloudUser {
			public UserInfo Info { get; set; }
			public string Password { get; set; }
		}

		public sealed class CloudBase {
			public int Id { get; set; }
			public string Name { get; set; }
			public string Title { get; set; }
			public string Version { get; set; }
			public string Guid { get; set; }
			public uint ProductId { get; set; }
			/// <summary>Наполнялась ли база - ClearDataBase сбрасывает флаг, не трогая реестр</summary>
			public bool HasData { get; set; }
		}

		public sealed class CloudAccess {
			public string Login { get; set; }
			public int BaseId { get; set; }
			public bool HasAccess { get; set; }
			public bool Admin { get; set; }
			public bool ReadOnly { get; set; }
		}

		public sealed class CloudState {
			public List<CloudUser> Users { get; } = new List<CloudUser>();
			public List<CloudBase> Bases { get; } = new List<CloudBase>();
			public List<CloudAccess> Access { get; } = new List<CloudAccess>();

			/// <summary>Версия, которую облако считает устаревшей - для проверки NeedUpdateLauncher</summary>
			public bool NeedUpdateLauncher { get; set; }

			/// <summary>Сессию к базе открыть не удастся - облако ответит отказом</summary>
			public bool RefuseSessions { get; set; }

			/// <summary>Сессия открывается, но без прав администратора базы</summary>
			public bool SessionWithoutAdmin { get; set; }

			/// <summary>Если задано - любой вызов управления падает этим кодом</summary>
			public StatusCode? FailEverythingWith { get; set; }

			/// <summary>Текст отказа, который облако кладёт в Status.Detail</summary>
			public string FailureDetail { get; set; }

			private int nextBaseId = 1;

			public CloudUser AddUser(string login, string password, bool isAdmin = false,
				string name = null, string email = null, bool disabled = false) {
				var user = new CloudUser {
					Password = password,
					Info = new UserInfo {
						Login = login, Name = name ?? string.Empty, Email = email ?? string.Empty,
						Phone = string.Empty, Post = string.Empty, Comment = string.Empty,
						Disabled = disabled, IsAccountAdmin = isAdmin
					}
				};
				Users.Add(user);
				return user;
			}

			public CloudBase AddBase(string name, string title = null, uint productId = 1,
				string version = "1.0", bool hasData = true) {
				var db = new CloudBase {
					Id = nextBaseId++, Name = name, Title = title ?? name, Version = version,
					Guid = System.Guid.NewGuid().ToString(), ProductId = productId, HasData = hasData
				};
				Bases.Add(db);
				return db;
			}

			public void Grant(string login, int baseId, bool admin = false, bool readOnly = false) {
				var row = Access.FirstOrDefault(a => a.Login == login && a.BaseId == baseId);
				if(row == null) {
					row = new CloudAccess { Login = login, BaseId = baseId };
					Access.Add(row);
				}
				row.HasAccess = true;
				row.Admin = admin;
				row.ReadOnly = readOnly;
			}

			public CloudUser FindUser(string login) =>
				Users.FirstOrDefault(u => string.Equals(u.Info.Login, login, StringComparison.OrdinalIgnoreCase));

			public CloudBase FindBase(int id) => Bases.FirstOrDefault(b => b.Id == id);

			public CloudAccess FindAccess(string login, int baseId) =>
				Access.FirstOrDefault(a =>
					string.Equals(a.Login, login, StringComparison.OrdinalIgnoreCase) && a.BaseId == baseId);

			public int NextBaseId => nextBaseId;
		}

		#endregion

		#region Общее для служб

		/// <summary>
		/// Разбирает заголовок Basic и проверяет пароль. Как настоящее облако, отвечает
		/// Unauthenticated при неверных данных - на этом держатся тесты неудачного входа.
		/// </summary>
		private static CloudUser Authenticate(CloudState state, ServerCallContext context) {
			string header = context.RequestHeaders.GetValue("authorization");
			if(string.IsNullOrEmpty(header) || !header.StartsWith("Basic ", StringComparison.Ordinal))
				throw new RpcException(new Status(StatusCode.Unauthenticated, "Нет заголовка авторизации"));

			string pair = Encoding.UTF8.GetString(Convert.FromBase64String(header.Substring("Basic ".Length)));
			int separator = pair.IndexOf(':');
			string account = pair.Substring(0, separator);
			string password = pair.Substring(separator + 1);
			// логин приходит в виде "аккаунт\логин"
			string login = account.Contains('\\') ? account.Split('\\')[1] : account;

			var user = state.FindUser(login);
			if(user == null || user.Password != password)
				throw new RpcException(new Status(StatusCode.Unauthenticated, "Неверный логин или пароль"));
			if(user.Info.Disabled)
				throw new RpcException(new Status(StatusCode.PermissionDenied, "Учётная запись отключена"));

			return user;
		}

		private static void ThrowIfFailureRequested(CloudState state) {
			if(state.FailEverythingWith == null)
				return;
			throw new RpcException(new Status(state.FailEverythingWith.Value,
				state.FailureDetail ?? "Облако недоступно"));
		}

		private static CloudUser Enter(CloudState state, ServerCallContext context) {
			ThrowIfFailureRequested(state);
			return Authenticate(state, context);
		}

		private static void RequireAdmin(CloudUser user) {
			if(!user.Info.IsAccountAdmin)
				throw new RpcException(new Status(StatusCode.PermissionDenied,
					"Недостаточно прав: нужен администратор аккаунта"));
		}

		#endregion

		#region Службы

		private sealed class LoginService : LoginManagement.LoginManagementBase {
			private readonly CloudState state;
			public LoginService(CloudState state) => this.state = state;

			public override Task<StartResponse> Start(StartRequest request, ServerCallContext context) {
				var user = Enter(state, context);
				return Task.FromResult(new StartResponse {
					YouAccountAdmin = user.Info.IsAccountAdmin,
					NeedUpdateLauncher = state.NeedUpdateLauncher
				});
			}

			public override Task<GetBasesForUserResponse> GetBasesForUser(
				GetBasesForUserRequest request, ServerCallContext context) {
				var user = Enter(state, context);

				var response = new GetBasesForUserResponse();
				var visible = state.Bases
					.Where(b => b.ProductId == request.ProductId)
					.Where(b => user.Info.IsAccountAdmin || state.FindAccess(user.Info.Login, b.Id)?.HasAccess == true);

				foreach(var db in visible)
					response.Bases.Add(new BaseInfo {
						BaseId = db.Id, BaseName = db.Name, BaseTitle = db.Title, BaseVersion = db.Version ?? string.Empty
					});
				return Task.FromResult(response);
			}

			public override Task<StartSessionResponse> StartSession(
				StartSessionRequest request, ServerCallContext context) {
				var user = Enter(state, context);

				if(state.RefuseSessions)
					return Task.FromResult(new StartSessionResponse {
						Success = false, Description = "Сессии временно недоступны"
					});

				var db = state.FindBase(request.BaseId);
				if(db == null)
					return Task.FromResult(new StartSessionResponse {
						Success = false, Description = "База не найдена"
					});

				return Task.FromResult(new StartSessionResponse {
					Success = true,
					SessionId = $"session-{db.Id}",
					IsAdmin = !state.SessionWithoutAdmin
						&& (user.Info.IsAccountAdmin || state.FindAccess(user.Info.Login, db.Id)?.Admin == true),
					Db = new BaseConnection {
						Login = "db_user", Password = "db_pass", Server = "db.example",
						BaseName = db.Name, Port = 3306
					}
				});
			}

			public override Task<ChangePasswordResponse> ChangePassword(
				ChangePasswordRequest request, ServerCallContext context) {
				var user = Enter(state, context);
				user.Password = request.NewPassword;
				return Task.FromResult(new ChangePasswordResponse { Success = true });
			}
		}

		private sealed class DataBaseService : DataBaseManagement.DataBaseManagementBase {
			private readonly CloudState state;
			public DataBaseService(CloudState state) => this.state = state;

			public override Task<CheckDataBaseExistsResponse> CheckDataBaseExists(
				CheckDataBaseExistsRequest request, ServerCallContext context) {
				Enter(state, context);
				var db = state.Bases.FirstOrDefault(b =>
					b.Name == request.Name && b.ProductId == request.ProductId);

				var response = new CheckDataBaseExistsResponse { Exists = db != null };
				if(db != null)
					response.BaseId = db.Id;
				return Task.FromResult(response);
			}

			public override Task<CreateDataBaseResponse> CreateDataBase(
				CreateDataBaseRequest request, ServerCallContext context) {
				var user = Enter(state, context);
				RequireAdmin(user);

				var db = state.AddBase(request.Name, request.Title, request.ProductId, version: null, hasData: false);
				state.Grant(user.Info.Login, db.Id, admin: true);
				return Task.FromResult(new CreateDataBaseResponse { BaseId = db.Id, BaseGuid = db.Guid });
			}

			public override Task<DropDataBaseResponse> DropDataBase(
				DropDataBaseRequest request, ServerCallContext context) {
				var user = Enter(state, context);
				RequireAdmin(user);

				var db = state.FindBase(request.BaseId);
				if(db == null)
					return Task.FromResult(new DropDataBaseResponse { Success = false });

				state.Bases.Remove(db);
				// облако чистит реестр вместе с базой
				state.Access.RemoveAll(a => a.BaseId == db.Id);
				return Task.FromResult(new DropDataBaseResponse { Success = true });
			}

			public override Task<ClearDataBaseResponse> ClearDataBase(
				ClearDataBaseRequest request, ServerCallContext context) {
				var user = Enter(state, context);
				RequireAdmin(user);

				var db = state.FindBase(request.BaseId);
				if(db == null)
					return Task.FromResult(new ClearDataBaseResponse { Success = false });

				// база пересоздаётся пустой, записи реестра и доступы остаются
				db.HasData = false;
				return Task.FromResult(new ClearDataBaseResponse { Success = true });
			}
		}

		private sealed class UserService : UserManagement.UserManagementBase {
			private readonly CloudState state;
			public UserService(CloudState state) => this.state = state;

			public override Task<GetUsersResponse> GetUsers(GetUsersRequest request, ServerCallContext context) {
				var user = Enter(state, context);
				RequireAdmin(user);

				var response = new GetUsersResponse();
				foreach(var known in state.Users)
					response.Users.Add(known.Info.Clone());
				return Task.FromResult(response);
			}

			public override Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context) {
				var user = Enter(state, context);
				RequireAdmin(user);

				if(state.FindUser(request.Login) != null)
					return Task.FromResult(new CreateUserResponse {
						Success = false, Message = "Пользователь с таким логином уже существует"
					});

				state.AddUser(request.Login, request.Password, request.IsAccountAdmin, request.Name, request.Email);
				var created = state.FindUser(request.Login);
				created.Info.Phone = request.Phone;
				created.Info.Post = request.Post;
				created.Info.Comment = request.Comment;
				return Task.FromResult(new CreateUserResponse { Success = true });
			}

			public override Task<UpdateUserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context) {
				var actor = Enter(state, context);
				RequireAdmin(actor);

				var target = state.FindUser(request.Login);
				if(target == null)
					return Task.FromResult(new UpdateUserResponse { Success = false, Message = "Пользователь не найден" });

				target.Info.Name = request.Name;
				target.Info.Email = request.Email;
				target.Info.Phone = request.Phone;
				target.Info.Post = request.Post;
				target.Info.Comment = request.Comment;
				target.Info.Disabled = request.Disabled;
				target.Info.IsAccountAdmin = request.IsAccountAdmin;
				if(!string.IsNullOrEmpty(request.NewPassword))
					target.Password = request.NewPassword;

				return Task.FromResult(new UpdateUserResponse { Success = true });
			}

			public override Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context) {
				var actor = Enter(state, context);
				RequireAdmin(actor);

				var target = state.FindUser(request.User);
				if(target == null)
					return Task.FromResult(new DeleteUserResponse { Success = false, Message = "Пользователь не найден" });

				state.Users.Remove(target);
				state.Access.RemoveAll(a => string.Equals(a.Login, request.User, StringComparison.OrdinalIgnoreCase));
				return Task.FromResult(new DeleteUserResponse { Success = true });
			}

			public override Task<GetUserBaseAccessResponse> GetUserBaseAccess(
				GetUserBaseAccessRequest request, ServerCallContext context) {
				var actor = Enter(state, context);
				RequireAdmin(actor);

				var response = new GetUserBaseAccessResponse();
				// облако отдаёт все базы продукта с флагом доступа у каждой
				foreach(var db in state.Bases.Where(b => b.ProductId == request.ProductId)) {
					var access = state.FindAccess(request.User, db.Id);
					response.Bases.Add(new BaseAccessInfo {
						BaseId = db.Id,
						BaseTitle = db.Title,
						HasAccess = access?.HasAccess == true,
						Admin = access?.Admin == true,
						ReadOnly = access?.ReadOnly == true
					});
				}
				return Task.FromResult(response);
			}

			public override Task<ChangeBaseAccessResponse> ChangeBaseAccess(
				ChangeBaseAccessRequest request, ServerCallContext context) {
				var actor = Enter(state, context);
				RequireAdmin(actor);

				if(state.FindUser(request.User) == null)
					return Task.FromResult(new ChangeBaseAccessResponse {
						Success = false, Message = "Пользователь не найден"
					});
				if(state.FindBase(request.BaseId) == null)
					return Task.FromResult(new ChangeBaseAccessResponse {
						Success = false, Message = "База не найдена"
					});

				if(!request.Grant)
					state.Access.RemoveAll(a =>
						string.Equals(a.Login, request.User, StringComparison.OrdinalIgnoreCase)
						&& a.BaseId == request.BaseId);
				else
					state.Grant(request.User, request.BaseId, request.Admin, request.ReadOnly);

				return Task.FromResult(new ChangeBaseAccessResponse { Success = true });
			}
		}

		/// <summary>
		/// Держит поток keep-alive открытым. Лаунчер запускает его фоном после StartSession,
		/// и без ответившей службы он молча уходит в переподключение.
		/// </summary>
		private sealed class SessionService : SessionManagement.SessionManagementBase {
			public override async Task Alive(AliveRequest request,
				IServerStreamWriter<AliveResponse> responseStream, ServerCallContext context) {
				try {
					await Task.Delay(Timeout.Infinite, context.CancellationToken);
				}
				catch(OperationCanceledException) {
					// закрытие сессии - штатное завершение потока
				}
			}
		}

		#endregion
	}
}
