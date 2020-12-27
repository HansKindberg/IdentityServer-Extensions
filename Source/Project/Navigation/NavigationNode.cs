using System;
using System.Collections.Generic;

namespace HansKindberg.IdentityServer.Navigation
{
	public class NavigationNode : INavigationNode
	{
		#region Constructors

		public NavigationNode(INavigationNode parent)
		{
			this.Parent = parent;
		}

		#endregion

		#region Properties

		public virtual bool Active { get; set; }
		IEnumerable<INavigationNode> INavigationNode.Children => this.Children;
		public virtual IList<NavigationNode> Children { get; } = new List<NavigationNode>();
		public virtual INavigationNode Parent { get; }
		public virtual string Text { get; set; }
		public virtual string Tooltip { get; set; }
		public virtual Uri Url { get; set; }

		#endregion
	}
}