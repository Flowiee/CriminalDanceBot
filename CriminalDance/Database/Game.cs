//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class Game
    {
        public int Id { get; set; }
        public int GrpId { get; set; }
        public string GroupName { get; set; }
        public long GroupId { get; set; }
        public Nullable<System.DateTime> TimeStarted { get; set; }
        public Nullable<System.DateTime> TimeEnded { get; set; }
        public string WinningTeam { get; set; }
        public string SpecialWinner { get; set; }
    
        public virtual Group Group { get; set; }
    }
}
