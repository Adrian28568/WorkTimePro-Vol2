using System;
using System.Collections.Generic;

namespace WorkTimePro.Api.Models.Entities
{
    /// <summary>
    /// Represents a user in the system
    /// Can be either a Worker or an Administrator
    /// </summary>
    public class AppUser
    {
        // ═══════════════════════════════════════════════════════════
        // BASIC IDENTITY
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Unique ID - automatically assigned by database
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Login username (must be unique)
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Encrypted password using BCrypt hashing
        /// NEVER store passwords in plain text!
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Is this user an administrator?
        /// true = Admin (can see all workers, view reports)
        /// false = Worker (can only see own data)
        /// </summary>
        public bool IsAdmin { get; set; }

        // ═══════════════════════════════════════════════════════════
        // PAYROLL INFORMATION
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Hourly wage in Euros
        /// Default: €13.00 per hour
        /// Used to calculate monthly earnings
        /// </summary>
        public decimal HourlyRate { get; set; } = 13.00m;

        // ═══════════════════════════════════════════════════════════
        // RE-LOGIN APPROVAL SYSTEM
        // Prevents workers from logging out and back in on same day
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Does this worker need admin approval to clock in again?
        /// Set to true when they logout during the day
        /// Admin must manually approve before they can re-enter
        /// </summary>
        public bool NeedsReloginApproval { get; set; } = false;

        /// <summary>
        /// Date when this user last logged out
        /// Used to check if re-login attempt is on same day
        /// </summary>
        public DateTime? LastLogoutDate { get; set; }

        // ═══════════════════════════════════════════════════════════
        // RELATIONSHIPS
        // Links to other database tables
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// All work sessions for this user
        /// One user can have many work sessions (one-to-many relationship)
        /// </summary>
        public ICollection<WorkSession> WorkSessions { get; set; }
            = new List<WorkSession>();
    }
}