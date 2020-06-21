using System;
using System.Threading.Tasks;
using Octokit.GraphQL;
using Octokit.GraphQL.Core;
using Octokit.GraphQL.Model;

namespace GprTool
{
    public class GraphQLUtilities
    {
        public static async Task<IQueryableList<Package>> FindPackageList(IConnection connection, string packagesPath)
        {
            var packageConnection = await FindPackageConnection(connection, packagesPath);
            return packageConnection?.AllPages();
        }

        static async Task<PackageConnection> FindPackageConnection(IConnection connection, string packagesPath)
        {
            var split = packagesPath.Split('/');
            var owner = split.Length > 0 ? split[0] : null;
            var repo = split.Length > 1 ? split[1] : null;
            var names = split.Length > 2 ? new [] { split[2] } : null;

            if (repo is string)
            {
                return new Query().Repository(owner: owner, name: repo).Packages(names: names);
            }

            try
            {
                var query = new Query().User(owner).Packages().Select(p => p.TotalCount).Compile();
                var total = await connection.Run(query);
                return new Query().User(owner).Packages();
            }
            catch (GraphQLException e) when (e.Message.StartsWith("Could not resolve to a "))
            {
                // continue looking
            }
            catch (GraphQLException e)
            {
                throw new ApplicationException(e.Message, e);
            }            

            try
            {
                var query = new Query().Organization(owner).Packages().Select(p => p.TotalCount).Compile();
                var total = await connection.Run(query);
                return new Query().Organization(owner).Packages();
            }
            catch (GraphQLException e) when (e.Message.StartsWith("Could not resolve to a "))
            {
                // continue looking
            }
            catch (GraphQLException e)
            {
                throw new ApplicationException(e.Message, e);
            }            

            return null;
        }
    }
}