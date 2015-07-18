/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System.Collections.Generic;
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Projects.Wrappers;

namespace ASC.Api.Projects
{
    public partial class ProjectApi
    {
        #region tags

        ///<summary>
        ///Returns the list of all available project tags
        ///</summary>
        ///<short>
        ///Project tags
        ///</short>
        /// <category>Tags</category>
        ///<returns>List of tags</returns>
        [Read(@"tag")]
        public IEnumerable<ObjectWrapperBase> GetAllTags()
        {
            return EngineFactory.GetTagEngine().GetTags().Select(x => new ObjectWrapperBase {Id = x.Key, Title = x.Value}).ToSmartList();
        }

        ///<summary>
        ///Returns the detailed list of all projects with the specified tag
        ///</summary>
        ///<short>
        ///Project by tag
        ///</short>
        /// <category>Tags</category>
        ///<param name="tag">Tag name</param>
        ///<returns>List of projects</returns>
        [Read(@"tag/{tag}")]
        public IEnumerable<ProjectWrapper> GetProjectsByTags(string tag)
        {
            var projectsTagged = EngineFactory.GetTagEngine().GetTagProjects(tag);
            return EngineFactory.GetProjectEngine().GetByID(projectsTagged).Select(x => new ProjectWrapper(x)).ToSmartList();
        }


        ///<summary>
        ///Returns the list of all tags like the specified tag name
        ///</summary>
        ///<short>
        ///Tags by tag name
        ///</short>
        /// <category>Tags</category>
        ///<param name="tagName">Tag name</param>
        ///<returns>List of tags</returns>
        [Read(@"tag/search")]
        public string[] GetTagsByName(string tagName)
        {
            return !string.IsNullOrEmpty(tagName) && tagName.Trim() != string.Empty
                       ? EngineFactory.GetTagEngine().GetTags(tagName.Trim()).Select(r => r.Value).ToArray()
                       : new string[0];
        }

        #endregion
    }
}