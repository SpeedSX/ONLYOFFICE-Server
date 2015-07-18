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


using System;
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Specific;

namespace ASC.Api.Projects.Wrappers
{
    [DataContract(Name = "project", Namespace = "")]
    public class ProjectWrapperFull : ObjectWrapperFullBase
    {
        [DataMember]
        public bool CanEdit { get; set; }

        [DataMember]
        public ProjectSecurityInfo Security { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public object ProjectFolder { get; set; }

        [DataMember(Order = 32)]
        public bool IsPrivate { get; set; }

        [DataMember(Order = 33)]
        public int TaskCount { get; set; }

        [DataMember(Order = 34)]
        public int MilestoneCount { get; set; }

        [DataMember(Order = 34)]
        public int DiscussionCount { get; set; }

        [DataMember(Order = 35)]
        public int ParticipantCount { get; set; }

        [DataMember(Order = 35)]
        public string TimeTrackingTotal { get; set; }

        [DataMember(Order = 35)]
        public int DocumentsCount { get; set; }


        private ProjectWrapperFull()
        {
        }

        public ProjectWrapperFull(Project project, object filesRoot)
        {
            Id = project.ID;
            Title = project.Title;
            Description = project.Description;
            Status = (int)project.Status;
            Responsible = EmployeeWraper.Get(project.Responsible);
            Created = (ApiDateTime)project.CreateOn;
            CreatedBy = EmployeeWraper.Get(project.CreateBy);
            Updated = (ApiDateTime)project.LastModifiedOn;
            if (project.CreateBy != project.LastModifiedBy)
            {
                UpdatedBy = EmployeeWraper.Get(project.LastModifiedBy);
            }
            Security = new ProjectSecurityInfo(project);
            CanEdit = ProjectSecurity.CanEdit(project);
            ProjectFolder = filesRoot;
            IsPrivate = project.Private;

            TaskCount = project.TaskCount;
            MilestoneCount = project.MilestoneCount;
            DiscussionCount = project.DiscussionCount;
            TimeTrackingTotal = project.TimeTrackingTotal ?? "";
            DocumentsCount = project.DocumentsCount;
            ParticipantCount = project.ParticipantCount;
        }

        public ProjectWrapperFull(Project project) : this(project, 0)
        {
        }


        public static ProjectWrapperFull GetSample()
        {
            return new ProjectWrapperFull
                {
                    Id = 10,
                    Title = "Sample Title",
                    Description = "Sample description",
                    Status = (int)MilestoneStatus.Open,
                    Responsible = EmployeeWraper.GetSample(),
                    Created = ApiDateTime.GetSample(),
                    CreatedBy = EmployeeWraper.GetSample(),
                    Updated = ApiDateTime.GetSample(),
                    UpdatedBy = EmployeeWraper.GetSample(),
                    ProjectFolder = 13234
                };
        }
    }
}