using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EFQuerying.Tracking;

internal class Program
{
    private static void Main(string[] args)
    {
        using (var context = new BloggingContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        using (var context = new BloggingContext())
        {
            // seeding database
            context.Blogs.Add(new Blog { Url = "http://sample.com/blog", Posts = new List<Post>() { new Post() { Title = "AAAAA", Content = "aaaaaaaa", Rating = 2 }, new Post() { Title = "AAAAA2222", Content = "aaaaaaaa2222", Rating = 4 } } });
            context.Blogs.Add(new Blog { Url = "http://sample.com/another_blog", Posts = new List<Post>() { new Post() { Title = "BBBB", Content = "bbbbbbbbbbbbbbbbb", Rating = 3 } } });
            context.SaveChanges();
        }

        #region Tracking(default option)
        using (var context = new BloggingContext())
        {
            var blog = context.Blogs.SingleOrDefault(b => b.BlogId == 1);
            blog.Rating = 3;
            context.SaveChanges();
        }
        #endregion

        #region AsNoTracking
        using (var context = new BloggingContext())
        {
            
            var blog = context.Blogs.AsNoTracking().SingleOrDefault(b => b.BlogId == 1);
            blog.Rating = 2;
            context.SaveChanges();
        }
        #endregion

        #region AsNoTrackingWithIdentityResolution
        using (var context = new BloggingContext())
        {
            var data = context.Posts.AsNoTracking().Include(x => x.Blog).ToList();
            var blog1 = data.Single(x => x.PostId == 1).Blog;
            blog1.Rating = 1;
            var blog2 = data.Single(x => x.PostId == 2).Blog;
            //expected: blog1.Rating != blog2.Rating
        }

        using (var context = new BloggingContext())
        {
            var data = context.Posts.AsNoTrackingWithIdentityResolution().Include(x => x.Blog).ToList();
            var blog1 = data.Single(x => x.PostId == 1).Blog;
            blog1.Rating = 1;
            var blog2 = data.Single(x => x.PostId == 2).Blog;
            //expected: blog1.Rating == blog2.Rating (because blog2 and blog1 reference same instance
        }
        #endregion AsNoTrackingWithIdentityResolution


        using (var context = new BloggingContext())
        {
            #region ContextDefaultTrackingBehavior
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var blogs = context.Blogs.ToList();
            #endregion
        }

        using (var context = new BloggingContext())
        {
            #region CustomProjection1
            var blog = context.Blogs
                .Select(
                    b =>
                        new { Blog = b, PostCount = b.Posts.Count() });
            #endregion
        }

        using (var context = new BloggingContext())
        {
            #region CustomProjection2
            var blog = context.Blogs
                .Select(
                    b =>
                        new { Blog = b, Post = b.Posts.OrderBy(p => p.Rating).LastOrDefault() });
            #endregion
        }

        using (var context = new BloggingContext())
        {
            #region CustomProjection3
            var blog = context.Blogs
                .Select(
                    b =>
                        new { Id = b.BlogId, b.Url });
            #endregion
        }

        using (var context = new BloggingContext())
        {
            #region ClientProjection
            var blogs = context.Blogs
                .OrderByDescending(blog => blog.Rating)
                .Select(
                    blog => new { Id = blog.BlogId, Url = StandardizeUrl(blog) })
                .ToList();
            #endregion
        }
    }

    #region ClientMethod
    public static string StandardizeUrl(Blog blog)
    {
        var url = blog.Url.ToLower();

        if (!url.StartsWith("http://"))
        {
            url = string.Concat("http://", url);
        }

        return url;
    }
    #endregion
}
