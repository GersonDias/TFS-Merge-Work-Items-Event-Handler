using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Server;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.VersionControl.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Services.Location.Server;
using BranchObject = Microsoft.TeamFoundation.VersionControl.Client.BranchObject;
using Change = Microsoft.TeamFoundation.VersionControl.Client.Change;
using Changeset = Microsoft.TeamFoundation.VersionControl.Server.Changeset;
using ChangeType = Microsoft.TeamFoundation.VersionControl.Client.ChangeType;
using RecursionType = Microsoft.TeamFoundation.VersionControl.Client.RecursionType;

namespace Inmeta.TFS.MergeWorkItemsEventHandler
{
	public static class Extensions
	{
		public static BranchObject GetBranchByFilePath(this VersionControlServer vcs, string mergeFilePath)
		{
			var branchObjects = vcs.QueryRootBranchObjects(RecursionType.Full);
			var branch = branchObjects.SingleOrDefault(b =>
			{
				var branchPath = b.Properties.RootItem.Item;
				return mergeFilePath.StartsWith(branchPath.EndsWith("/") ? branchPath : branchPath + "/");
			});

			return branch;
		}

		public static Changeset GetChangeset(this TeamFoundationRequestContext requestContext, int changesetId)
		{
			TeamFoundationVersionControlService service = requestContext.GetService<TeamFoundationVersionControlService>();
			TeamFoundationDataReader teamFoundationDataReader = service.QueryChangeset(requestContext, changesetId, true, false, true);
			return teamFoundationDataReader.Current<Changeset>();
		}

	    public static TfsTeamProjectCollection GetCollection(this TeamFoundationRequestContext requestContext)
	    {
	        var service = requestContext.GetService<ILocationService>();
	        var accessMapping = service.GetServerAccessMapping(requestContext);
	        Uri selfReferenceUri = service.GetSelfReferenceUri(requestContext, accessMapping);
	        return new TfsTeamProjectCollection(selfReferenceUri);
	    }

	    public static TfsTeamProjectCollection GetImpersonatedCollection(this TeamFoundationRequestContext requestContext, string userToImpersonate)
		{
			var service = requestContext.GetService<ILocationService>();
			Uri selfReferenceUri = service.GetSelfReferenceUri(requestContext, service.GetServerAccessMapping(requestContext));
			return ImpersonatedCollection.CreateImpersonatedCollection(selfReferenceUri, userToImpersonate);
		}

		public static IEnumerable<Change> PendingMerges(this Change[] changes)
		{
			return 
				from ch in changes
				where (ch.ChangeType & ChangeType.Merge) == ChangeType.Merge
				select ch;
		}
		public static bool ContainsArtifact(this LinkCollection links, string artifactUri)
		{
			return (
				from l in links.OfType<ExternalLink>()
				select l).Any((ExternalLink el) => el.LinkedArtifactUri == artifactUri);
		}
	}
}
