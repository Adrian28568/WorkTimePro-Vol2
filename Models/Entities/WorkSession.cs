using System;

namespace WorkTimePro.Api.Models.Entities
{
    /// <summary>
    /// Represents one work session for an employee
    /// Each time a worker clocks in, a new WorkSession is created
    /// </summary>
    public class WorkSession
    {
        // ═══════════════════════════════════════════════════════════
        // BASIC INFO
        // ═══════════════════════════════════════════════════════════
        
        public int Id { get; set; }

        /// <summary>
        /// Foreign Key - which worker owns this session
        /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Navigation property - allows accessing user data
        /// Example: session.User.Username
        /// </summary>
        public AppUser User { get; set; } = null!;

        // ═══════════════════════════════════════════════════════════
        // TIME TRACKING
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// When worker clocked in (Central European Time)
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// When worker clocked out
        /// NULL = still working (session active)
        /// </summary>
        public DateTime? EndTime { get; set; }

        // ───────────────────────────────────────────────────────────
        // BREAK TRACKING (3 fields work together)
        // ───────────────────────────────────────────────────────────
        
        /// <summary>
        /// Total accumulated break time in minutes
        /// Updated each time worker resumes from break
        /// Example: 3 breaks of 10, 15, 20 min = 45 total
        /// </summary>
        public int PausedMinutes { get; set; } = 0;

        /// <summary>
        /// Is worker currently on break?
        /// true = on break right now
        /// false = actively working
        /// </summary>
        public bool IsPaused { get; set; } = false;

        /// <summary>
        /// When did current break start?
        /// Used to calculate ongoing break duration
        /// NULL = not on break
        /// </summary>
        public DateTime? CurrentPauseStartTime { get; set; }

        // ═══════════════════════════════════════════════════════════
        // STATUS
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Has this work session been completed?
        /// true = worker clocked out, session finished
        /// false = still active (working or paused)
        /// </summary>
        public bool IsFinished { get; set; } = false;

        // ═══════════════════════════════════════════════════════════
        // COMPUTED PROPERTY - ACTUAL WORKED TIME
        // This is NOT stored in database - it's calculated on-the-fly
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// Total worked minutes EXCLUDING all breaks
        /// 
        /// CALCULATION:
        /// 1. Total Time = (End Time OR Now) - Start Time
        /// 2. Subtract accumulated breaks (PausedMinutes)
        /// 3. If currently on break, also subtract ongoing break
        /// 
        /// EXAMPLE:
        /// Started: 09:00, Now: 13:00 (4 hours = 240 min)
        /// Past breaks: 25 min total
        /// Current break: Started 15 min ago
        /// Worked = 240 - 25 - 15 = 200 min (3h 20m)
        /// </summary>
        public int WorkedMinutes
        {
            get
            {
                // Get current time in Central European timezone
                var cet = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cet);
                
                // Use end time if session finished, otherwise use current time
                var endTimeToUse = EndTime ?? now;
                
                // Calculate total elapsed time
                var totalMinutes = (int)(endTimeToUse - StartTime).TotalMinutes;
                
                // Subtract all accumulated break time
                var workedMinutes = totalMinutes - PausedMinutes;
                
                // If currently on break, also subtract the ongoing break
                if (IsPaused && CurrentPauseStartTime.HasValue)
                {
                    var ongoingPauseMinutes = (int)(now - CurrentPauseStartTime.Value).TotalMinutes;
                    workedMinutes -= ongoingPauseMinutes;
                }
                
                // Never return negative (safety check)
                return Math.Max(0, workedMinutes);
            }
        }

        // ═══════════════════════════════════════════════════════════
        // METADATA
        // ═══════════════════════════════════════════════════════════
        
        /// <summary>
        /// When this record was last updated
        /// Used to track when session state changes
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}