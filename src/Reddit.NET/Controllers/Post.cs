﻿using RedditThings = Reddit.NET.Models.Structures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Reddit.NET.Controllers
{
    /// <summary>
    /// Base class for posts and comments.
    /// </summary>
    public abstract class Post : BaseController
    {
        public string Subreddit;
        public string Title;
        public string Author;
        public string Id;
        public string Fullname;
        public string Permalink;
        public DateTime Created;
        public DateTime Edited;
        public int Score;
        public int UpVotes;
        public int DownVotes;
        public bool Removed;
        public bool Spam;
        public bool NSFW;

        /// <summary>
        /// The full Listing object returned by the Reddit API;
        /// </summary>
        public RedditThings.Post Listing;

        public List<Comment> Comments
        {
            get
            {
                return (CommentsLastUpdated.HasValue
                    && CommentsLastUpdated.Value.AddSeconds(15) > DateTime.Now ? comments : GetComments());
            }
            private set
            {
                comments = value;
            }
        }
        private List<Comment> comments;
        private DateTime? CommentsLastUpdated;

        internal readonly Dispatch Dispatch;

        public Post(Dispatch dispatch, RedditThings.Post listing)
        {
            Import(listing);
            Dispatch = dispatch;
        }

        public Post(Dispatch dispatch, string subreddit, string title, string author, string id = null, string name = null, string permalink = null,
            DateTime created = default(DateTime), DateTime edited = default(DateTime), int score = 0, int upVotes = 0,
            int downVotes = 0, bool removed = false, bool spam = false, bool nsfw = false)
        {
            Import(subreddit, title, author, id, name, permalink, created, edited, score, upVotes, downVotes, removed, spam, nsfw);
            Dispatch = dispatch;
        }

        public Post(Dispatch dispatch, string name)
        {
            Dispatch = dispatch;
            Fullname = name;
        }

        public Post(Dispatch dispatch)
        {
            Dispatch = dispatch;
        }

        internal void Import(RedditThings.Post listing)
        {
            Subreddit = listing.Subreddit;
            Title = listing.Title;
            Author = listing.Author;
            Id = listing.Id;
            Fullname = listing.Name;
            Permalink = listing.Permalink;
            Created = listing.Created;
            Edited = listing.Edited;
            Score = listing.Score;
            UpVotes = listing.Ups;
            DownVotes = listing.Downs;
            Removed = listing.Removed;
            Spam = listing.Spam;
            NSFW = listing.Over18;

            Listing = listing;
        }

        internal void Import(string subreddit, string title, string author, string id = null, string name = null, string permalink = null,
            DateTime created = default(DateTime), DateTime edited = default(DateTime), int score = 0, int upVotes = 0,
            int downVotes = 0, bool removed = false, bool spam = false, bool nsfw = false)
        {
            Subreddit = subreddit;
            Title = title;
            Author = author;
            Id = id;
            Fullname = name;
            Permalink = permalink;
            Created = created;
            Edited = edited;
            Score = score;
            UpVotes = upVotes;
            DownVotes = downVotes;
            Removed = removed;
            Spam = spam;
            NSFW = nsfw;

            Listing = new RedditThings.Post(this);
        }

        abstract public RedditThings.PostResultShortData Submit(bool resubmit = false, bool ad = false, string app = "", string extension = "",
            string flairId = "", string flairText = "", string gRecapthaResponse = "", bool sendReplies = true, bool spoiler = false,
            string videoPosterUrl = "");

        public Comment Comment(string author, string body, string bodyHtml = null,
            string collapsedReason = null, bool collapsed = false, bool isSubmitter = false,
            List<Comment> replies = null, bool scoreHidden = false, int depth = 0, string id = null, string name = null,
            string permalink = null, DateTime created = default(DateTime), DateTime edited = default(DateTime),
            int score = 0, int upVotes = 0, int downVotes = 0, bool removed = false, bool spam = false)
        {
            return new Comment(Dispatch, Subreddit, author, body, Fullname, bodyHtml, collapsedReason, collapsed, isSubmitter, replies, scoreHidden,
                depth, id, name, permalink, created, edited, score, upVotes, downVotes, removed, spam);
        }

        public Comment Comment()
        {
            return new Comment(Dispatch, Subreddit, null, null, Fullname);
        }

        /// <summary>
        /// Retrieve comment replies to this post.
        /// </summary>
        /// <param name="sort">one of (confidence, top, new, controversial, old, random, qa, live)</param>
        /// <param name="context">an integer between 0 and 8</param>
        /// <param name="truncate">an integer between 0 and 50</param>
        /// <param name="showEdits">boolean value</param>
        /// <param name="showMore">boolean value</param>
        /// <param name="threaded">boolean value</param>
        /// <param name="depth">(optional) an integer</param>
        /// <param name="limit">(optional) an integer</param>
        /// <param name="srDetail">(optional) expand subreddits</param>
        /// <returns>A list of comments.</returns>
        public List<Comment> GetComments(string sort = "new", int context = 3, int truncate = 0, bool showEdits = false, bool showMore = true,
            bool threaded = true, int? depth = null, int? limit = null, bool srDetail = false)
        {
            List<Comment> comments = GetComments(Dispatch.Listings.GetComments(Id, context, showEdits, showMore, sort, threaded, truncate, Subreddit, null,
                depth, limit, srDetail), Dispatch);

            CommentsLastUpdated = DateTime.Now;

            Comments = comments;
            return comments;
        }

        /// <summary>
        /// Delete this post.
        /// </summary>
        public void Delete()
        {
            Dispatch.LinksAndComments.Delete(Fullname);
        }

        /// <summary>
        /// Delete this post asynchronously.
        /// </summary>
        public async void DeleteAsync()
        {
            await Task.Run(() =>
            {
                Delete();
            });
        }

        /// <summary>
        /// Hide this post.
        /// This removes it from the user's default view of subreddit listings.
        /// </summary>
        public void Hide()
        {
            Dispatch.LinksAndComments.Hide(Fullname);
        }

        /// <summary>
        /// Hide this post asynchronously.
        /// This removes it from the user's default view of subreddit listings.
        /// </summary>
        public async void HideAsync()
        {
            await Task.Run(() =>
            {
                Hide();
            });
        }

        /// <summary>
        /// Lock this post.
        /// Prevents a post from receiving new comments.
        /// </summary>
        public void Lock()
        {
            Dispatch.LinksAndComments.Lock(Fullname);
        }

        /// <summary>
        /// Lock this post asynchronously.
        /// Prevents a post from receiving new comments.
        /// </summary>
        public async void LockAsync()
        {
            await Task.Run(() =>
            {
                Lock();
            });
        }

        /// <summary>
        /// Mark this post as NSFW.
        /// </summary>
        public void MarkNSFW()
        {
            Dispatch.LinksAndComments.MarkNSFW(Fullname);
        }

        /// <summary>
        /// Mark this post as NSFW asynchronously.
        /// </summary>
        public async void MarkNSFWAsync()
        {
            await Task.Run(() =>
            {
                MarkNSFW();
            });
        }

        /// <summary>
        /// Retrieve additional comments omitted from a base comment tree.
        /// When a comment tree is rendered, the most relevant comments are selected for display first.
        /// Remaining comments are stubbed out with "MoreComments" links. 
        /// This API call is used to retrieve the additional comments represented by those stubs, up to 100 at a time.
        /// The two core parameters required are link and children. link is the fullname of the link whose comments are being fetched. 
        /// children is a comma-delimited list of comment ID36s that need to be fetched.
        /// If id is passed, it should be the ID of the MoreComments object this call is replacing. This is needed only for the HTML UI's purposes and is optional otherwise.
        /// NOTE: you may only make one request at a time to this API endpoint. Higher concurrency will result in an error being returned.
        /// If limit_children is True, only return the children requested.
        /// </summary>
        /// <param name="children">a comma-delimited list of comment ID36s</param>
        /// <param name="limitChildren">boolean value</param>
        /// <param name="sort">one of (confidence, top, new, controversial, old, random, qa, live)</param>
        /// <param name="id">(optional) id of the associated MoreChildren object</param>
        /// <returns>The requested comments.</returns>
        public RedditThings.MoreChildren MoreChildren(string children, bool limitChildren, string sort, string id = null)
        {
            return Validate(Dispatch.LinksAndComments.MoreChildren(children, limitChildren, Fullname, sort, id));
        }

        /// <summary>
        /// Report this post to the subreddit moderators.  The post then becomes implicitly hidden, as well.
        /// </summary>
        /// <param name="additionalInfo">a string no longer than 2000 characters</param>
        /// <param name="banEvadingAccountsNames">a string no longer than 1000 characters</param>
        /// <param name="customText">a string no longer than 250 characters</param>
        /// <param name="fromHelpCenter">boolean value</param>
        /// <param name="otherReason">a string no longer than 100 characters</param>
        /// <param name="reason">a string no longer than 100 characters</param>
        /// <param name="ruleReason">a string no longer than 100 characters</param>
        /// <param name="siteReason">a string no longer than 100 characters</param>
        /// <param name="violatorUsername">A valid Reddit username</param>
        public void Report(string additionalInfo, string banEvadingAccountsNames, string customText, bool fromHelpCenter,
            string otherReason, string reason, string ruleReason, string siteReason, string violatorUsername)
        {
            Validate(Dispatch.LinksAndComments.Report(additionalInfo, banEvadingAccountsNames, customText, fromHelpCenter, otherReason, reason,
                ruleReason, siteReason, Subreddit, Fullname, violatorUsername));
        }

        /// <summary>
        /// Report this post to the subreddit moderators asynchronously.  The post then becomes implicitly hidden, as well.
        /// </summary>
        /// <param name="additionalInfo">a string no longer than 2000 characters</param>
        /// <param name="banEvadingAccountsNames">a string no longer than 1000 characters</param>
        /// <param name="customText">a string no longer than 250 characters</param>
        /// <param name="fromHelpCenter">boolean value</param>
        /// <param name="otherReason">a string no longer than 100 characters</param>
        /// <param name="reason">a string no longer than 100 characters</param>
        /// <param name="ruleReason">a string no longer than 100 characters</param>
        /// <param name="siteReason">a string no longer than 100 characters</param>
        /// <param name="violatorUsername">A valid Reddit username</param>
        public async void ReportAsync(string additionalInfo, string banEvadingAccountsNames, string customText, bool fromHelpCenter,
            string otherReason, string reason, string ruleReason, string siteReason, string violatorUsername)
        {
            await Task.Run(() =>
            {
                Report(additionalInfo, banEvadingAccountsNames, customText, fromHelpCenter, otherReason, reason, ruleReason, siteReason, violatorUsername);
            });
        }

        /// <summary>
        /// Save this post.
        /// Saved things are kept in the user's saved listing for later perusal.
        /// </summary>
        /// <param name="category">a category name</param>
        public void Save(string category)
        {
            Dispatch.LinksAndComments.Save(category, Fullname);
        }

        /// <summary>
        /// Save this post asynchronously.
        /// Saved things are kept in the user's saved listing for later perusal.
        /// </summary>
        /// <param name="category">a category name</param>
        public async void SaveAsync(string category)
        {
            await Task.Run(() =>
            {
                Save(category);
            });
        }

        /// <summary>
        /// Enable inbox replies for this post.
        /// </summary>
        public void EnableSendReplies()
        {
            Dispatch.LinksAndComments.SendReplies(Fullname, true);
        }

        /// <summary>
        /// Enable inbox replies for this post asynchronously.
        /// </summary>
        public async void EnableSendRepliesAsync()
        {
            await Task.Run(() =>
            {
                EnableSendReplies();
            });
        }

        /// <summary>
        /// Disable inbox replies for this post.
        /// </summary>
        public void DisableSendReplies()
        {
            Dispatch.LinksAndComments.SendReplies(Fullname, false);
        }

        /// <summary>
        /// Disable inbox replies for this post asynchronously.
        /// </summary>
        public async void DisableSendRepliesAsync()
        {
            await Task.Run(() =>
            {
                DisableSendReplies();
            });
        }

        /// <summary>
        /// Enable contest mode for this post.
        /// </summary>
        public void EnableContestMode()
        {
            Dispatch.LinksAndComments.SetContestMode(Fullname, true);
        }

        /// <summary>
        /// Enable contest mode for this post asynchronously.
        /// </summary>
        public async void EnableContestModeAsync()
        {
            await Task.Run(() =>
            {
                EnableContestMode();
            });
        }

        /// <summary>
        /// Disable contest mode for this post.
        /// </summary>
        public void DisableContestMode()
        {
            Dispatch.LinksAndComments.SetContestMode(Fullname, false);
        }

        /// <summary>
        /// Disable contest mode for this post asynchronously.
        /// </summary>
        public async void DisableContestModeAsync()
        {
            await Task.Run(() =>
            {
                DisableContestMode();
            });
        }

        /// <summary>
        /// Set this post as the sticky in its subreddit.
        /// The num argument is optional, and only used when stickying a post.
        /// It allows specifying a particular "slot" to sticky the post into, and if there is already a post stickied in that slot it will be replaced.
        /// If there is no post in the specified slot to replace, or num is None, the bottom-most slot will be used.
        /// </summary>
        /// <param name="num">an integer between 1 and 4</param>
        /// <param name="toProfile">boolean value</param>
        public void SetSubredditSticky(int num, bool toProfile)
        {
            Validate(Dispatch.LinksAndComments.SetSubredditSticky(Fullname, num, true, toProfile));
        }

        /// <summary>
        /// Set this post as the sticky in its subreddit asynchronously.
        /// The num argument is optional, and only used when stickying a post.
        /// It allows specifying a particular "slot" to sticky the post into, and if there is already a post stickied in that slot it will be replaced.
        /// If there is no post in the specified slot to replace, or num is None, the bottom-most slot will be used.
        /// </summary>
        /// <param name="num">an integer between 1 and 4</param>
        /// <param name="toProfile">boolean value</param>
        public async void SetSubredditStickyAsync(int num, bool toProfile)
        {
            await Task.Run(() =>
            {
                SetSubredditSticky(num, toProfile);
            });
        }

        /// <summary>
        /// Unset this post as the sticky in its subreddit.
        /// The num argument is optional, and only used when stickying a post.
        /// It allows specifying a particular "slot" to sticky the post into, and if there is already a post stickied in that slot it will be replaced.
        /// If there is no post in the specified slot to replace, or num is None, the bottom-most slot will be used.
        /// </summary>
        /// <param name="num">an integer between 1 and 4</param>
        /// <param name="toProfile">boolean value</param>
        public void UnsetSubredditSticky(int num, bool toProfile)
        {
            Validate(Dispatch.LinksAndComments.SetSubredditSticky(Fullname, num, false, toProfile));
        }

        /// <summary>
        /// Unset this post as the sticky in its subreddit asynchronously.
        /// The num argument is optional, and only used when stickying a post.
        /// It allows specifying a particular "slot" to sticky the post into, and if there is already a post stickied in that slot it will be replaced.
        /// If there is no post in the specified slot to replace, or num is None, the bottom-most slot will be used.
        /// </summary>
        /// <param name="num">an integer between 1 and 4</param>
        /// <param name="toProfile">boolean value</param>
        public async void UnsetSubredditStickyAsync(int num, bool toProfile)
        {
            await Task.Run(() =>
            {
                UnsetSubredditSticky(num, toProfile);
            });
        }

        /// <summary>
        /// Set a suggested sort for this post.
        /// Suggested sorts are useful to display comments in a certain preferred way for posts.
        /// For example, casual conversation may be better sorted by new by default, or AMAs may be sorted by Q&A.
        /// A sort of an empty string clears the default sort.
        /// </summary>
        /// <param name="sort">one of (confidence, top, new, controversial, old, random, qa, live, blank)</param>
        public void SetSuggestedSort(string sort)
        {
            Validate(Dispatch.LinksAndComments.SetSuggestedSort(Fullname, sort));
        }

        /// <summary>
        /// Set a suggested sort for this post asynchronously.
        /// Suggested sorts are useful to display comments in a certain preferred way for posts.
        /// For example, casual conversation may be better sorted by new by default, or AMAs may be sorted by Q&A.
        /// A sort of an empty string clears the default sort.
        /// </summary>
        /// <param name="sort">one of (confidence, top, new, controversial, old, random, qa, live, blank)</param>
        public async void SetSuggestedSortAsync(string sort)
        {
            await Task.Run(() =>
            {
                SetSuggestedSort(sort);
            });
        }

        /// <summary>
        /// Mark this post as containing spoilers.
        /// </summary>
        public void Spoiler()
        {
            Dispatch.LinksAndComments.Spoiler(Fullname);
        }

        /// <summary>
        /// Mark this post as containing spoilers asynchronously.
        /// </summary>
        public async void SpoilerAsync()
        {
            await Task.Run(() =>
            {
                Spoiler();
            });
        }

        /// <summary>
        /// Unhide this post.
        /// </summary>
        public void Unhide()
        {
            Dispatch.LinksAndComments.Unhide(Fullname);
        }

        /// <summary>
        /// Unhide this post asynchronously.
        /// </summary>
        public async void UnhideAsync()
        {
            await Task.Run(() =>
            {
                Unhide();
            });
        }

        /// <summary>
        /// Unlock this post.
        /// Allows this post to receive new comments.
        /// </summary>
        public void Unlock()
        {
            Dispatch.LinksAndComments.Unlock(Fullname);
        }

        /// <summary>
        /// Unlock this post asynchronously.
        /// Allows this post to receive new comments.
        /// </summary>
        public async void UnlockAsync()
        {
            await Task.Run(() =>
            {
                Unlock();
            });
        }

        /// <summary>
        /// Remove the NSFW marking from this post.
        /// </summary>
        public void UnmarkNSFW()
        {
            Dispatch.LinksAndComments.UnmarkNSFW(Fullname);
        }

        /// <summary>
        /// Remove the NSFW marking from this post asynchronously.
        /// </summary>
        public async void UnmarkNSFWAsync()
        {
            await Task.Run(() =>
            {
                UnmarkNSFW();
            });
        }

        /// <summary>
        /// Unsave this post.
        /// This removes the thing from the user's saved listings as well.
        /// </summary>
        public void Unsave()
        {
            Dispatch.LinksAndComments.Unsave(Fullname);
        }

        /// <summary>
        /// Unsave this post asynchronously.
        /// This removes the thing from the user's saved listings as well.
        /// </summary>
        public async void UnsaveAsync()
        {
            await Task.Run(() =>
            {
                Unsave();
            });
        }

        /// <summary>
        /// Remove spoiler.
        /// </summary>
        public void Unspoiler()
        {
            Dispatch.LinksAndComments.Unspoiler(Fullname);
        }

        /// <summary>
        /// Remove spoiler asynchronously.
        /// </summary>
        public async void UnspoilerAsync()
        {
            await Task.Run(() =>
            {
                Unspoiler();
            });
        }

        // TODO - Add vote methods (up/down) once Model tested.  --Kris

        /// <summary>
        /// Approve this post.
        /// If the thing was removed, it will be re-inserted into appropriate listings.
        /// Any reports on the approved thing will be discarded.
        /// </summary>
        public void Approve()
        {
            Dispatch.Moderation.Approve(Fullname);
        }
    }
}
