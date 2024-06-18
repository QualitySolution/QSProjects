namespace QS.Project.Services 
{
	public class GtkApplicationQuitService : IApplicationQuitService 
	{
		public void Quit() 
		{
			Gtk.Application.Quit();
		}
	}
}
