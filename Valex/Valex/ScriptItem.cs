using System.Windows.Controls;
using System.Windows.Markup;
using Valex.Assets.Classes;

namespace Valex;

public partial class ScriptItem : UserControl, IComponentConnector
{
	public ScriptItem(string Title, string Tag, string Views, string Likes, string Script, WebSocketServer api)
	{
		InitializeComponent();
		title.Text = Title;
		desc.Text = Tag;
		views.Text = Views;
		likes.Text = Likes;
		execute.Click += delegate
		{
			api.Execute(Script);
		};
	}
}
