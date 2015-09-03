using HRI.Models;
using Umbraco.Core;
using Umbraco.Core.Persistence;

namespace HRI.Services
{
    /// <summary>
    /// Deployment service to make DB changes
    /// </summary>
    public class DeployService
    {
        private readonly UmbracoDatabase _dbContext = ApplicationContext.Current.DatabaseContext.Database ;

        public void Install()
        {
            InstallPasswordHistoryFeature();
        }

        private void InstallPasswordHistoryFeature()
        {
            //Check if the DB table does NOT exist
            if (!_dbContext.TableExist("hriPasswordsHistory"))
            {
                //Create DB table - and set overwrite to false
                _dbContext.CreateTable<PasswordHistory>(false);
            }
        }
    }
}