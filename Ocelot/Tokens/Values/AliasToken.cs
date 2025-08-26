using System;
using Pooshit.Ocelot.Entities.Descriptors;
using Pooshit.Ocelot.Entities.Operations.Prepared;
using Pooshit.Ocelot.Info;

namespace Pooshit.Ocelot.Tokens.Values;

/// <summary>
/// token used 
/// </summary>
public class AliasToken : SqlToken {
	
	/// <summary>
	/// creates a new <see cref="AliasToken"/>
	/// </summary>
	/// <param name="token">token for which to create an alias</param>
	/// <param name="alias">alias to create for token</param>
	public AliasToken(ISqlToken token, string alias) {
		Token = token;
		Alias = alias;
	}

	/// <summary>
	/// token for which to create an alias
	/// </summary>
	public ISqlToken Token { get; set; }

	/// <summary>
	/// alias to create for token
	/// </summary>
	public string Alias { get; set; }

	/// <inheritdoc />
	public override void ToSql(IDBInfo dbinfo, IOperationPreparator preparator, Func<Type, EntityDescriptor> models, string tablealias) {
		Token.ToSql(dbinfo, preparator, models, tablealias);
		preparator.AppendText("AS");
		preparator.AppendText(Alias);
	}
}