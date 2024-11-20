using GameDB;
using Microsoft.EntityFrameworkCore;

namespace ConsoleTest
{
    // Create Read Update Delete
    internal class Program
    {
        static void Main(string[] args)
        {
            // Create
            //using (GameDbContext db = new GameDbContext())
            //{
            //    TestDb testDb = new TestDb();
            //    testDb.Name = "King";

            //    db.Tests.Add(testDb);

            //    db.SaveChanges();
            //}

            // Read
            //using (GameDbContext db = new GameDbContext())
            //{
            //    TestDb? findDb = db.Tests.FirstOrDefault(t => t.Name == "King");
            //    if (findDb != null)
            //    {
            //        int check = findDb.TestDbId;
            //    }
            //}

            // Update
            //using (GameDbContext db = new GameDbContext())
            //{
            //    TestDb? findDb = db.Tests.FirstOrDefault(t => t.Name == "King");
            //    if(findDb != null)
            //    {
            //        findDb.Name = "King" + findDb.TestDbId;
            //        db.SaveChanges();
            //    }
            //}

            // Delete
            using (GameDbContext db = new GameDbContext())
            {
                TestDb? findDb = db.Tests.FirstOrDefault(t => t.Name == "King");
                if (findDb != null)
                {
                    db.Tests.Remove(findDb);
                    db.SaveChanges();
                }

                // Entity Tracking
                {
                    TestDb testDb = new TestDb()
                    {
                        TestDbId = 1
                    };

                    var entry = db.Tests.Entry(testDb);
                    entry.State = EntityState.Deleted;
                    db.SaveChanges();
                }

                // EF Core 7
                {
                    db.Tests.Where(a => a.Name.Contains("King")).ExecuteDelete();
                }
            }
        }
    }
}
