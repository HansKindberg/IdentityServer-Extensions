namespace HansKindberg.IdentityServer
{
	public enum UriFactoryQueryMode
	{
		/// <summary>
		/// Includes all items in the query from the current http-context.
		/// </summary>
		All,

		/// <summary>
		/// Includes no items in the query from the current http-context.
		/// </summary>
		None,

		/// <summary>
		/// Includes the ui-locales item in the query from the current http-context.
		/// </summary>
		UiLocales
	}
}