using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace HRI.Models
{
    [TableName("hriPasswordsHistory")]
    [PrimaryKey("Id", autoIncrement = true)]
    [ExplicitColumns]
    public class PasswordHistory
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("MemberId")]
        public int MemberId { get; set; }

        [Column("EncryptedPassword")]
        public string EncryptedPassword { get; set; }

        [Column("ChangeDate")]
        public DateTime ChangeDate { get; set; }
    }
}