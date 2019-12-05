using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PocketPermaculture.Data;

namespace PocketPermaculture.Data
{
    // Extends base Identity User
    public class ApplicationUser : IdentityUser
    {
        public virtual IList<UserAddress> UserAddresses { get; set; }
        public virtual IList<UserPin> UserPins { get; set; }
    }

    public class UserAddress
    {
        [Key]
        public string Id { get; set; }

        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int PostalCode { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
    }

    public class UserPin
    {
        [Key]
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PinCategory { get; set; }
        public string PinType { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
