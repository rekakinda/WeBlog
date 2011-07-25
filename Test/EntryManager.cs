﻿using System;
using System.Linq;
using System.Web;
using NUnit.Framework;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using System.IO;
using Mod = Sitecore.Modules.WeBlog.Managers;
using Sitecore.Data;
using Sitecore.Search;

namespace Sitecore.Modules.WeBlog.Test
{
    [TestFixture]
    [Category("EntryManager")]
    public class EntryManager
    {
        private Item m_testRoot = null;
        private Item m_blog1 = null;
        private Item m_entry11 = null;
        private Item m_entry12 = null;
        private Item m_entry13 = null;
        private Item m_category12 = null;
        private Item m_category13 = null;
        private Item m_blog2 = null;
        private Item m_entry21 = null;
        private Item m_entry22 = null;
        private Item m_entry23 = null;
        private Item m_category21 = null;
        private Item m_category22 = null;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            // Create test content
            var home = Sitecore.Context.Database.GetItem("/sitecore/content/home");
            using (new SecurityDisabler())
            {
                home.Paste(File.ReadAllText(HttpContext.Current.Server.MapPath(@"~\test data\entry manager content.xml")), false, PasteMode.Overwrite);
            }

            // Retrieve created content items
            m_testRoot = home.Axes.GetChild("weblog testroot");
            m_blog1 = m_testRoot.Axes.GetChild("blog1");
            m_blog2 = m_testRoot.Axes.GetChild("blog2");

            m_entry11 = m_blog1.Axes.GetDescendant("Entry1");
            m_entry12 = m_blog1.Axes.GetDescendant("Entry2");
            m_entry13 = m_blog1.Axes.GetDescendant("Entry3");
            m_entry21 = m_blog2.Axes.GetDescendant("Entry1");
            m_entry22 = m_blog2.Axes.GetDescendant("Entry2");
            m_entry23 = m_blog2.Axes.GetDescendant("Entry3");

            var blog1Categories = m_blog1.Axes.GetChild("categories");
            m_category12 = blog1Categories.Axes.GetChild("category2");
            m_category13 = blog1Categories.Axes.GetChild("category3");

            var blog2Categories = m_blog2.Axes.GetChild("categories");
            m_category21 = blog2Categories.Axes.GetChild("category1");
            m_category22 = blog2Categories.Axes.GetChild("category2");

            // rebuild the WeBlog search index (or the entry manager won't work)
            var index = SearchManager.GetIndex(Sitecore.Modules.WeBlog.Constants.Index.Name);
            index.Rebuild();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            using (new SecurityDisabler())
            {
                if (m_testRoot != null)
                    m_testRoot.Delete();
            }
        }

        [Test]
        public void GetBlogEntries_Blog1()
        {
            var entryIds = (from entry in Mod.EntryManager.GetBlogEntries(m_blog1, int.MaxValue, null, null)
                            select entry.ID).ToArray();

            Assert.AreEqual(3, entryIds.Length);
            Assert.Contains(m_entry11.ID, entryIds);
            Assert.Contains(m_entry12.ID, entryIds);
            Assert.Contains(m_entry13.ID, entryIds);
        }

        [Test]
        public void GetBlogEntries_Blog2()
        {
            var entryIds = (from entry in Mod.EntryManager.GetBlogEntries(m_blog2, int.MaxValue, null, null)
                            select entry.ID).ToArray();

            Assert.AreEqual(3, entryIds.Length);
            Assert.Contains(m_entry21.ID, entryIds);
            Assert.Contains(m_entry22.ID, entryIds);
            Assert.Contains(m_entry23.ID, entryIds);
        }

        [Test]
        public void GetBlogEntries_Blog1_WithLimit()
        {
            var entryIds = (from entry in Mod.EntryManager.GetBlogEntries(m_blog1, 2, null, null)
                            select entry.ID).ToArray();

            Assert.AreEqual(2, entryIds.Length);
            Assert.Contains(m_entry12.ID, entryIds);
            Assert.Contains(m_entry13.ID, entryIds);
        }

        [Test]
        public void GetBlogEntries_Blog1_WithTag()
        {
            var entryIds = (from entry in Mod.EntryManager.GetBlogEntries(m_blog1, int.MaxValue, "tagb", null)
                            select entry.ID).ToArray();

            Assert.AreEqual(2, entryIds.Length);
            Assert.Contains(m_entry11.ID, entryIds);
            Assert.Contains(m_entry12.ID, entryIds);
        }

        [Test]
        public void GetBlogEntries_TagWithSpace()
        {
            var entryIds = (from entry in Mod.EntryManager.GetBlogEntries(m_blog1, int.MaxValue, "tag with space", null)
                            select entry.ID).ToArray();

            Assert.AreEqual(1, entryIds.Length);
            Assert.Contains(m_entry13.ID, entryIds);
        }

        [Test]
        public void GetBlogEntries_Blog1_WithCategory()
        {
            var entryIds = (from entry in Mod.EntryManager.GetBlogEntries(m_blog1, int.MaxValue, null, m_category13.Name)
                            select entry.ID).ToArray();

            Assert.AreEqual(1, entryIds.Length);
            Assert.Contains(m_entry13.ID, entryIds);
        }

        [Test]
        public void GetBlogEntries_Blog1_WithLimitAndCategory()
        {
            var entryIds = (from entry in Mod.EntryManager.GetBlogEntries(m_blog1, 1, null, m_category12.Name)
                            select entry.ID).ToArray();

            Assert.AreEqual(1, entryIds.Length);
            Assert.Contains(m_entry13.ID, entryIds);
        }

        [Test]
        public void GetBlogEntries_Blog1_NonBlogItem()
        {
            var entryIds = (from entry in Mod.EntryManager.GetBlogEntries(m_entry11, int.MaxValue, null, null)
                            select entry.ID).ToArray();

            Assert.AreEqual(0, entryIds.Length);
        }

        [Test]
        public void GetBlogEntries_Blog1_WithZeroLimit()
        {
            var entryIds = (from entry in Mod.EntryManager.GetBlogEntries(m_blog1, 0, null, null)
                            select entry.ID).ToArray();

            Assert.AreEqual(0, entryIds.Length);
        }

        [Test]
        public void GetBlogEntries_Blog1_WithNegativeLimit()
        {
            var entryIds = (from entry in Mod.EntryManager.GetBlogEntries(m_blog1, -7, null, null)
                            select entry.ID).ToArray();

            Assert.AreEqual(0, entryIds.Length);
        }

        [Test]
        public void GetBlogEntries_Blog1_WithInvalidCategory()
        {
            var entryIds = (from entry in Mod.EntryManager.GetBlogEntries(m_blog1, int.MaxValue, null, "bler")
                            select entry.ID).ToArray();

            Assert.AreEqual(0, entryIds.Length);
        }

        [Test]
        public void GetBlogEntries_Blog1_WithInvalidTag()
        {
            var entryIds = (from entry in Mod.EntryManager.GetBlogEntries(m_blog1, int.MaxValue, "bler", null)
                            select entry.ID).ToArray();

            Assert.AreEqual(0, entryIds.Length);
        }

        [Test]
        public void GetBlogEntryByCategorie_Blog2_Category1_ById()
        {
            var entryIds = (from entry in Mod.EntryManager.GetBlogEntryByCategorie(m_blog2.ID, m_category21.ID)
                            select entry.ID).ToArray();

            Assert.AreEqual(2, entryIds.Length);
            Assert.Contains(m_entry21.ID, entryIds);
            Assert.Contains(m_entry22.ID, entryIds);
        }

        [Test]
        public void GetBlogEntryByCategorie_Blog2_Category1()
        {
            var entryIds = (from entry in Mod.EntryManager.GetBlogEntryByCategorie(m_blog2.ID, m_category21.Name)
                            select entry.ID).ToArray();

            Assert.AreEqual(2, entryIds.Length);
            Assert.Contains(m_entry21.ID, entryIds);
            Assert.Contains(m_entry22.ID, entryIds);
        }

        [Test]
        public void GetBlogEntryByCategorie_Blog2_InvalidCategory()
        {
            var entryIds = (from entry in Mod.EntryManager.GetBlogEntryByCategorie(m_blog2.ID, m_category12.ID)
                            select entry.ID).ToArray();

            Assert.AreEqual(0, entryIds.Length);
        }

        [Test]
        public void GetBlogEntriesByMonthAndYear_Blog1_March2011()
        {
            var entryIds = (from entry in Mod.EntryManager.GetBlogEntriesByMonthAndYear(m_blog1, 3, 2011)
                            select entry.ID).ToArray();

            Assert.AreEqual(2, entryIds.Length);
            Assert.Contains(m_entry11.ID, entryIds);
            Assert.Contains(m_entry12.ID, entryIds);
        }

        [Test]
        public void GetBlogEntriesByMonthAndYear_Blog1_April2011()
        {
            var entryIds = (from entry in Mod.EntryManager.GetBlogEntriesByMonthAndYear(m_blog1, 4, 2011)
                            select entry.ID).ToArray();

            Assert.AreEqual(1, entryIds.Length);
            Assert.Contains(m_entry13.ID, entryIds);
        }

        [Test]
        public void GetBlogEntriesByMonthAndYear_Blog1_InvalidMonth()
        {
            var entryIds = (from entry in Mod.EntryManager.GetBlogEntriesByMonthAndYear(m_blog1, 17, 2011)
                            select entry.ID).ToArray();

            Assert.AreEqual(0, entryIds.Length);
        }

        [Test]
        public void MakeSortedEntriesList_InOrder()
        {
            using (new SecurityDisabler())
            {
                m_testRoot.Paste(File.ReadAllText(HttpContext.Current.Server.MapPath(@"~\test data\entries in order.xml")), false, PasteMode.Overwrite);
            }

            // rebuild the WeBlog search index (or the entry manager won't work)
            var index = SearchManager.GetIndex(Sitecore.Modules.WeBlog.Constants.Index.Name);
            index.Rebuild();

            var blog = m_testRoot.Axes.GetChild("MyBlog");

            try
            {
                var entries = from entry in Mod.EntryManager.GetBlogEntries(blog)
                              select entry.InnerItem;

                var sorted = entries.ToArray();
                Assert.AreEqual(3, sorted.Length);
                Assert.AreEqual("Entry3", sorted[0].Name);
                Assert.AreEqual("Entry2", sorted[1].Name);
                Assert.AreEqual("Entry1", sorted[2].Name);
            }
            finally
            {
                if (blog != null)
                {
                    using (new SecurityDisabler())
                    {
                        blog.Delete();
                    }
                }
            }
        }

        [Test]
        public void MakeSortedEntriesList_ReverseOrder()
        {
            using (new SecurityDisabler())
            {
                m_testRoot.Paste(File.ReadAllText(HttpContext.Current.Server.MapPath(@"~\test data\entries reverse order.xml")), false, PasteMode.Overwrite);
            }

            // rebuild the WeBlog search index (or the entry manager won't work)
            var index = SearchManager.GetIndex(Sitecore.Modules.WeBlog.Constants.Index.Name);
            index.Rebuild();

            var blog = m_testRoot.Axes.GetChild("MyBlog");

            try
            {
                var entries = from entry in Mod.EntryManager.GetBlogEntries(blog)
                              select entry.InnerItem;

                var sorted = entries.ToArray();
                Assert.AreEqual(3, sorted.Length);
                Assert.AreEqual("Entry1", sorted[0].Name);
                Assert.AreEqual("Entry2", sorted[1].Name);
                Assert.AreEqual("Entry3", sorted[2].Name);
            }
            finally
            {
                if (blog != null)
                {
                    using (new SecurityDisabler())
                    {
                        blog.Delete();
                    }
                }
            }
        }

        [Test]
        public void MakeSortedEntriesList_OutOfOrder()
        {
            using (new SecurityDisabler())
            {
                m_testRoot.Paste(File.ReadAllText(HttpContext.Current.Server.MapPath(@"~\test data\entries out of order.xml")), false, PasteMode.Overwrite);
            }

            // rebuild the WeBlog search index (or the entry manager won't work)
            var index = SearchManager.GetIndex(Sitecore.Modules.WeBlog.Constants.Index.Name);
            index.Rebuild();

            var blog = m_testRoot.Axes.GetChild("MyBlog");

            try
            {
                var entries = from entry in Mod.EntryManager.GetBlogEntries(blog)
                              select entry.InnerItem;

                var sorted = entries.ToArray();
                Assert.AreEqual(3, sorted.Length);
                Assert.AreEqual("Yet another entry", sorted[0].Name);
                Assert.AreEqual("Another Entry", sorted[1].Name);
                Assert.AreEqual("First Entry", sorted[2].Name);
            }
            finally
            {
                if (blog != null)
                {
                    using (new SecurityDisabler())
                    {
                        blog.Delete();
                    }
                }
            }
        }

        [Test]
        public void DeleteEntry_Null()
        {
            Assert.IsFalse(Mod.EntryManager.DeleteEntry(null));
        }

        [Test]
        public void DeleteEntry_InValidID()
        {
            Assert.IsFalse(Mod.EntryManager.DeleteEntry(ID.NewID.ToString()));
        }

        [Test]
        public void DeleteEntry_ValidItem()
        {
            Item toDel = null;
            var template = Sitecore.Context.Database.GetTemplate(Sitecore.Configuration.Settings.GetSetting("Blog.EntryTemplateID"));

            try
            {
                using (new SecurityDisabler())
                {
                    toDel = m_testRoot.Add("todel", template);
                    Assert.IsNotNull(toDel);

                    Assert.IsTrue(Mod.EntryManager.DeleteEntry(toDel.ID.ToString()));
                }
            }
            finally
            {
                if (toDel != null)
                {
                    using (new SecurityDisabler())
                    {
                        toDel.Delete();
                    }
                }
            }
        }
    }
}