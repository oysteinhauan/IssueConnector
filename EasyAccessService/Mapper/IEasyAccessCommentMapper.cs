using EasyAccessService.Models;
using IssueConnectorLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyAccessService.Mapper
{
    public interface IEasyAccessCommentMapper
    {
		Task<IStandardComment> ToStandardComment(Comment comment, List<Comment> allComments, List<ViewPoint> viewpoints);

		Task<Comment> ToNewSpecialComment(IStandardComment comment);

		Task<Comment> ToUpdatedSpecialComment(IStandardComment instance, Comment oldSpecial);
	}
}
