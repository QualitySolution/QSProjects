using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.Utilities;
using QS.Utilities.Text;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;

namespace QS.Deletion.ViewModels
{
	public class DeletionViewModel : WindowDialogViewModelBase, IOnCloseActionViewModel
	{
		public readonly DeleteCore Deletion;
		public Action DeletionAccepted;

		public List<TreeNode> DeletedItems = new List<TreeNode>();
		public List<TreeNode> DependenceTree = new List<TreeNode>();

		public DeletionViewModel(INavigationManager navigation, DeleteCore deletion) : base(navigation)
		{
			this.Deletion = deletion;
			Title = "Выполнить удаление?";

			var deleteNode = new TreeNode();
			deleteNode.CountedNode = false;
			FillObgectGroups(deleteNode, Deletion.DeletedItems);
			if(deleteNode.TotalChildCount > 0) {
				deleteNode.Title = NumberToTextRus.FormatCase(deleteNode.Childs.Count,
					"Будут удалены объекты {0} вида",
					"Будут удалены объекты {0} видов",
					"Будут удалены объекты {0} видов");
				DeletedItems.Add(deleteNode);
			}

			var cleanNode = new TreeNode();
			cleanNode.CountedNode = false;
			FillObgectGroups(cleanNode, Deletion.CleanedItems);
			if (cleanNode.TotalChildCount > 0) {
				cleanNode.Title = NumberToTextRus.FormatCase(cleanNode.Childs.Count,
					"Будут очищены ссылки в объектах {0} вида",
					"Будут очищены ссылки в объектах {0} видов",
					"Будут очищены ссылки в объектах {0} видов");
				DeletedItems.Add(cleanNode);
			}

			var removeNode = new TreeNode();
			removeNode.CountedNode = false;
			FillObgectGroups(removeNode, Deletion.CleanedItems);
			if (removeNode.TotalChildCount > 0) {
				removeNode.Title = NumberToTextRus.FormatCase(removeNode.Childs.Count,
					"Будут очищены ссылки в коллекциях у {0} вида объектов",
					"Будут очищены ссылки в коллекциях у {0} видов объектов",
					"Будут очищены ссылки в коллекциях у {0} видов объектов");
				DeletedItems.Add(removeNode);
			}

			DeletedItems.Sort();

			var rootNode = FillTreeDependence(Deletion.RootEntity);
			DependenceTree.Add(rootNode);
		}

		#region Команды View

		public void RunDetetion()
		{
			Close(true, CloseSource.Self);
			DeletionAccepted();
		}

		public void CancelDeletion()
		{
			Close(true, CloseSource.Cancel);
		}

		#endregion

		#region Внутренние методы

		public TreeNode FillTreeDependence(EntityDTO entity, TreeNode parent = null)
		{
			var node = new TreeNode(entity.Title, parent, entity.Id);
			foreach(var item in entity.PullsUp) {
				FillTreeDependence(item, node);
			}
			node.Childs.Sort();
			return node;
		}

		public void FillObgectGroups(TreeNode rootNode, List<EntityDTO> entities) {
			foreach (var typeGroup in entities.GroupBy(x => x.ClassType)) {
				var names = typeGroup.Key.GetSubjectNames();
				var groupNode = new TreeNode(names.NominativePlural.StringToTitleCase(), rootNode);
				groupNode.CountedNode = false;
				foreach (var item in typeGroup.OrderBy(x => x.Title)) {
					var node = new TreeNode(item.Title, groupNode, item.Id);
				}
			}
			rootNode.Childs.Sort();
		}

		public void OnClose(CloseSource source)
		{
			if(source != CloseSource.Self)
				Deletion.DeletionExecuted = false;
		}

		#endregion
	}

	public class TreeNode : IComparable<TreeNode>
	{
		public uint Id { get; set; }
		public string Title { get; set; }
		public bool CountedNode = true;

		public TreeNode Parrent { get; set; }
		public List<TreeNode> Childs { get; } = new List<TreeNode>();

		public int TotalChildCount => Childs.Sum(x => (x.CountedNode ? 1 : 0) + x.TotalChildCount);

		public TreeNode(string title = null, TreeNode parrent = null, uint id = 0)
		{
			this.Title = title;
			this.Id = id;
			Parrent = parrent;
			if (Parrent != null)
				Parrent.Childs.Add(this);
		}

		public int CompareTo(TreeNode other)
		{
			var countResult = TotalChildCount.CompareTo(other.TotalChildCount);
			if(countResult != 0)
				return -countResult;
			return string.Compare(Title, other.Title, StringComparison.CurrentCulture);
		}
	}
}
