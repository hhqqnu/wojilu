﻿/*
 * Copyright (c) 2010, www.wojilu.com. All rights reserved.
 */

using System;

using wojilu.Web.Mvc;
using wojilu.Web.Mvc.Attr;
using wojilu.Apps.Forum.Domain;
using wojilu.Apps.Forum.Interface;
using wojilu.Apps.Forum.Service;
using wojilu.Web.Controller.Common;
using wojilu.Members.Users.Domain;

namespace wojilu.Web.Controller.Forum.Moderators {

    [App( typeof( ForumApp ) )]
    public class TopicSaveController : ControllerBase {


        public IForumTopicService topicService { get; set; }
        public IForumBoardService boardService { get; set; }
        public IForumCategoryService categoryService { get; set; }
        public IForumLogService logService { get; set; }
        public IForumService forumService { get; set; }

        public TopicSaveController() {
            boardService = new ForumBoardService();
            topicService = new ForumTopicService();
            categoryService = new ForumCategoryService();
            logService = new ForumLogService();
            forumService = new ForumService();
        }

        private String idList;
        private String condition;

        public override void CheckPermission() {

            this.idList = ctx.GetIdList( "ids" );
            this.condition = "Id in (" + idList + ")";

            if ("up".Equals( ctx.Post( "cmd" ) ) || "down".Equals( ctx.Post( "cmd" ) )) return;

            if (strUtil.IsNullOrEmpty( idList )) echoToParent( lang( "plsSelect" ) );
        }


        [HttpPost, DbTransaction]
        public void Sticky() {
            int id = ctx.GetInt( "boardId" );
            topicService.MakeSticky( av() );
            echoToParent( lang( "opok" ) );
        }

        [HttpPost, DbTransaction]
        public void StickyUndo() {
            int id = ctx.GetInt( "boardId" );
            topicService.MakeStickyUndo( av() );
            echoAjaxOk();
        }

        [HttpPost, DbTransaction]
        public void GlobalSticky() {
            int id = ctx.GetInt( "boardId" );
            topicService.MakeGlobalSticky( av() );
            echoToParent( lang( "opok" ) );
        }

        [HttpPost, DbTransaction]
        public void GlobalStickyUndo() {
            int id = ctx.GetInt( "boardId" );
            topicService.MakeGloablStickyUndo( av() );
            echoAjaxOk();
        }

        [HttpPost, DbTransaction]
        public void Pick() {
            int id = ctx.GetInt( "boardId" );
            topicService.MakePick( av() );
            echoToParent( lang( "opok" ) );
        }

        [HttpPost, DbTransaction]
        public void PickedUndo() {
            int id = ctx.GetInt( "boardId" );
            topicService.MakePickUndo( av() );
            echoAjaxOk();
        }

        [HttpPost, DbTransaction]
        public void Highlight() {
            int id = ctx.GetInt( "boardId" );
            topicService.MakeHighlight( strUtil.SqlClean( FormController.GetTitleStyle( ctx ), 150 ), av() );
            echoToParent( lang( "opok" ) );
        }

        [HttpPost, DbTransaction]
        public void HighlightUndo() {
            int id = ctx.GetInt( "boardId" );
            topicService.MakeHighlightUndo( av() );
            echoAjaxOk();
        }

        [HttpPost, DbTransaction]
        public void Lock() {
            int id = ctx.GetInt( "boardId" );
            topicService.MakeLock( av() );
            echoToParent( lang( "opok" ) );
        }

        [HttpPost, DbTransaction]
        public void LockUndo() {
            int id = ctx.GetInt( "boardId" );
            topicService.MakeLockUndo( av() );
            echoAjaxOk();
        }

        [HttpPost, DbTransaction]
        public void Delete() {
            int id = ctx.GetInt( "boardId" );
            topicService.DeleteList( av() );
            echoToParent( lang( "opok" ) );
        }

        [HttpPost, DbTransaction]
        public void Move() {
            int id = ctx.GetInt( "boardId" );
            int targetForumId = ctx.PostInt( "targetForum" );
            ForumBoard targetBoard = boardService.GetById( targetForumId, ctx.owner.obj );
            ctx.SetItem( "targetForumId", targetForumId );

            if (targetBoard == null) {
                errors.Add( alang( "exBoardNotFound" ) );
                return;
            }

            if (targetBoard.IsCategory == 1) {
                errors.Add( alang( "exTargetCantCategory" ) );
                return;
            }

            topicService.MakeMove( targetBoard.Id, av() );
            echoToParent( lang( "opok" ) );
        }

        [HttpPost, DbTransaction]
        public void SaveStickySort() {

            int id = ctx.GetInt( "boardId" );
            ForumBoard bd = boardService.GetById( id, ctx.owner.obj );

            int topicId = ctx.PostInt( "id" );
            String cmd = ctx.Post( "cmd" );
            if (cmd == "up") {
                topicService.StickyMoveUp( topicId );
                echoRedirect( "ok" );
            }
            else if (cmd == "down") {
                topicService.StickyMoveDown( topicId );
                echoRedirect( "ok" );
            }
            else {
                errors.Add( lang( "exUnknowCmd" ) );
                echoError();
            }
        }


        [HttpPost, DbTransaction]
        public void SaveGlobalStickySort() {

            int topicId = ctx.PostInt( "id" );
            String cmd = ctx.Post( "cmd" );

            ForumApp app = ctx.app.obj as ForumApp;

            if (cmd == "up") {

                forumService.StickyMoveUp( app, topicId );
                echoRedirect( "ok" );
            }
            else if (cmd == "down") {
                forumService.StickyMoveDown( app, topicId );
                echoRedirect( "ok" );
            }
            else {
                errors.Add( lang( "exUnknowCmd" ) );
                echoError();
            }
        }

        [HttpPost, DbTransaction]
        public void Category() {

            int id = ctx.GetInt( "boardId" );
            int categoryId = ctx.PostInt( "dropCategories" );
            ForumCategory category = categoryService.GetById( categoryId, ctx.owner.obj );
            if (category == null && categoryId > 0) {
                echoText( "<h1>" + alang( "exCategoryNotFound" ) + "</h4>" );
                return;
            }

            topicService.MakeCategory( categoryId, av() );
            echoToParent( lang( "opok" ) );
        }

        //-----------------------------------------------------------------------------------------------------

        private AdminValue av() {

            AdminValue v = new AdminValue();

            v.Ids = idList;
            v.AppId = ctx.app.Id;

            v.User = (User)ctx.viewer.obj;
            v.Reason = ctx.PostIsCheck( "chkReason" ) == 1 ? ctx.Post( "reasonText" ) : ctx.Post( "reasonSelect" );

            v.Ip = ctx.Ip;
            v.IsSendMsg = ctx.PostIsCheck( "IsSendMsg" ) == 1;

            return v;
        }

    }

}
