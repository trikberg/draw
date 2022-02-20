using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Draw.Server.Controllers
{
    public static class GitCommitHash
    {
        private static string commitHash = null;
        public static string CommitHash
        {
            get
            {
                if (commitHash == null)
                {
                    try
                    {
                        commitHash = GetCommitHash();
                    }
                    catch (Exception)
                    {
                        commitHash = "";
                    }
                }
                return commitHash;
            }
        }

        private static string GetCommitHash()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream resourceStream = assembly.GetManifestResourceStream("Draw.Server.Resources.commit.txt");

            if (resourceStream == null)
            {
                throw new InvalidOperationException("Missing embedded resource 'commit.txt'");
            }

            try
            {
                using (StreamReader reader = new StreamReader(resourceStream, Encoding.UTF8))
                {
                    return reader.ReadLine();
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error reading embedded resource 'commit.txt'", e);

            }
        }
    }
}
