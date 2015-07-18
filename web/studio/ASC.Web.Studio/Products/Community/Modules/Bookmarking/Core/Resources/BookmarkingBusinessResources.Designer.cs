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


//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.36323
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASC.Bookmarking.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class BookmarkingBusinessResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal BookmarkingBusinessResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ASC.Web.Community.Modules.Bookmarking.Core.Resources.BookmarkingBusinessResources" +
                            "", typeof(BookmarkingBusinessResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to bookmark added to favorites.
        /// </summary>
        public static string BookmarkAddedToFavouritesAction {
            get {
                return ResourceManager.GetString("BookmarkAddedToFavouritesAction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to new bookmark created.
        /// </summary>
        public static string BookmarkCreatedAction {
            get {
                return ResourceManager.GetString("BookmarkCreatedAction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to bookmark removed from favorites.
        /// </summary>
        public static string BookmarkRemovedAction {
            get {
                return ResourceManager.GetString("BookmarkRemovedAction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to new comment added.
        /// </summary>
        public static string CommentCreatedAction {
            get {
                return ResourceManager.GetString("CommentCreatedAction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The comment has been edited.
        /// </summary>
        public static string CommentModifiedAction {
            get {
                return ResourceManager.GetString("CommentModifiedAction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The comment has been deleted.
        /// </summary>
        public static string CommentRemovedAction {
            get {
                return ResourceManager.GetString("CommentRemovedAction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Subscription to bookmarks.
        /// </summary>
        public static string SubscriptionTypeNewBookmark {
            get {
                return ResourceManager.GetString("SubscriptionTypeNewBookmark", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Subscription to comments.
        /// </summary>
        public static string SubscriptionTypeNewComments {
            get {
                return ResourceManager.GetString("SubscriptionTypeNewComments", resourceCulture);
            }
        }
    }
}
