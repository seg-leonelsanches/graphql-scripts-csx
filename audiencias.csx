#r "nuget: GraphQL, 7.4.1"
#r "nuget: GraphQL.Client, 6.0.0"
#r "nuget: GraphQL.Client.Serializer.Newtonsoft, 6.0.0"

using Internal;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;

public class ApiGatewayResponseData
{
    public Workspace Workspace { get; set; }
}

public class Workspace
{
    public List<Space> Spaces { get; set; }
}

public class Space
{
    public String Id { get; set; }
    public String Name { get; set; }
    public AudiencesData Audiences { get; set; }
}

public class AudiencesData
{
    public List<Audience> Data { get; set; }
}

public class Audience
{
    public String Id { get; set; }
    public String Key { get; set; }
    public String Name { get; set; }
    public UInt32 Size { get; set; }
    public String CreatedAt { get; set; }
    public User CreatedBy { get; set; }
    public AudienceDefinition Definition { get; set; }
}

public class User
{
    public String Email { get; set; }
}

public class AudienceDefinition
{
    public AudienceDefinitionOptions Options { get; set; }
}

public class AudienceDefinitionOptions
{
    public AudienceDefinitionAstNode Ast { get; set; }
}

public class AudienceDefinitionAstNode
{
    public String Type { get; set; }
    public String Value { get; set; }
    public List<AudienceDefinitionAstNode> Children { get; set; }
    public AudienceDefinitionAstNodeOptions Options { get; set; }
}

public class AudienceDefinitionAstNodeOptions
{
    public String ConditionType { get; set; }
}

public static class Functions
{
		public static void TraverseAst(AudienceDefinitionAstNode node, Int32 indent = 0)
		{
        String indentLevel = new String(' ', indent);

        Console.WriteLine($"{indentLevel}Type: {node.Type}");
        Console.WriteLine($"{indentLevel}Value: {node.Value}");
        Console.WriteLine($"{indentLevel}Children: ");

        foreach (var child in node.Children ?? new List<AudienceDefinitionAstNode>())
        {
            TraverseAst(child, indent + 2);
        }
    }
	}

String bearerToken = "sua-bearer-token-aqui";
var _client = new GraphQLHttpClient("https://gateway-api.segment.com/graphql", new NewtonsoftJsonSerializer());
_client.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");

var query = new GraphQLRequest
{
    Query = @"
                query spacesQuery {
                  workspace(slug: ""xp-investimentos"") {
                    spaces {
                      id
                      name
                      audiences {
                        data {
                          id
                          key
                          name
                          size
                          createdAt
                          createdBy {
                            email
                          }
                          definition {
                            options
                          }
                        }
                      }
                    }
                  }
                }",
};

var response = await _client.SendQueryAsync<ApiGatewayResponseData>(query);

/*
 * O objeto `ApiGatewayResponseData` contém o objeto `Workspace`, que contém o
 * restante das informações que interessam para o resultado. 
 * 
 * Aqui eu defino um laço de iteração que chama `Functions.TraverseAst`, um 
 * método estático de exemplo que uso para mostrar como iterar pelos elementos
 * obtidos pela pesquisa no GraphQL. 
 */
foreach (var space in response.Data.Workspace.Spaces)
{
    foreach (var audience in space.Audiences.Data)
    {
        Console.WriteLine($"Audience: {audience.Name}");
        Console.WriteLine($"Size: {audience.Size}");
        Console.WriteLine($"Created At: {audience.CreatedAt}");
        Console.WriteLine($"Created By: {audience.CreatedBy.Email}");
        Console.WriteLine("---");
        Console.WriteLine("Definition:");
        Functions.TraverseAst(audience.Definition.Options.Ast);
        Console.WriteLine("---");
    }
}