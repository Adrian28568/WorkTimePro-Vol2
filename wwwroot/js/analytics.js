// ========== ANALYTICS.JS - Charts and Data Visualization ==========

const API_URL = "http://localhost:5286";

let trendsChart = null;
let dayOfWeekChart = null;
let peakHoursChart = null;
let dailyPatternChart = null;

function checkAuth() {
    const userStr = localStorage.getItem("user");
    if (!userStr) {
        window.location.href = "/login.html";
        return null;
    }
    
    const user = JSON.parse(userStr);
    if (!user.isAdmin) {
        alert("Access denied. Admin only.");
        window.location.href = "/login.html";
        return null;
    }
    
    return user;
}

function logout() {
    localStorage.removeItem("user");
    window.location.href = "/login.html";
}

// ═══════════════════════════════════════════════════════════════
// LOAD MONTHLY TRENDS
// ═══════════════════════════════════════════════════════════════

async function loadTrends() {
    const months = document.getElementById("trendMonths").value;
    
    try {
        const response = await fetch(`${API_URL}/api/admin/trends?months=${months}`);
        
        if (!response.ok) {
            throw new Error("Failed to fetch trends");
        }
        
        const data = await response.json();
        
        // Prepare chart data
        const labels = data.trends.map(t => t.month);
        const totalHours = data.trends.map(t => t.totalHours);
        const workersActive = data.trends.map(t => t.workersActive);
        
        // Destroy existing chart if it exists
        if (trendsChart) {
            trendsChart.destroy();
        }
        
        // Create trends chart
        const ctx = document.getElementById('trendsChart').getContext('2d');
        trendsChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Total Hours',
                        data: totalHours,
                        borderColor: '#667eea',
                        backgroundColor: 'rgba(102, 126, 234, 0.1)',
                        tension: 0.4,
                        fill: true,
                        yAxisID: 'y'
                    },
                    {
                        label: 'Active Workers',
                        data: workersActive,
                        borderColor: '#f093fb',
                        backgroundColor: 'rgba(240, 147, 251, 0.1)',
                        tension: 0.4,
                        fill: true,
                        yAxisID: 'y1'
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    mode: 'index',
                    intersect: false,
                },
                plugins: {
                    legend: {
                        position: 'top',
                    },
                    title: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        type: 'linear',
                        display: true,
                        position: 'left',
                        title: {
                            display: true,
                            text: 'Total Hours'
                        }
                    },
                    y1: {
                        type: 'linear',
                        display: true,
                        position: 'right',
                        title: {
                            display: true,
                            text: 'Active Workers'
                        },
                        grid: {
                            drawOnChartArea: false,
                        },
                    },
                }
            }
        });
        
    } catch (err) {
        console.error("Trends error:", err);
    }
}

// ═══════════════════════════════════════════════════════════════
// LOAD PRODUCTIVITY ANALYTICS
// ═══════════════════════════════════════════════════════════════

async function loadAnalytics() {
    const month = document.getElementById("monthSelect").value;
    const year = document.getElementById("yearSelect").value;
    
    try {
        const response = await fetch(`${API_URL}/api/admin/productivity?month=${month}&year=${year}`);
        
        if (!response.ok) {
            throw new Error("Failed to fetch productivity data");
        }
        
        const data = await response.json();
        
        // Update summary cards
        document.getElementById("totalHours").innerText = data.summary.totalHours + "h";
        document.getElementById("totalSessions").innerText = data.summary.totalSessions;
        document.getElementById("mostProductiveDay").innerText = data.summary.mostProductiveDay || "-";
        document.getElementById("peakHour").innerText = data.summary.peakClockInHour || "-";
        
        // Update Day of Week Chart
        updateDayOfWeekChart(data.dayOfWeekStats);
        
        // Update Peak Hours Chart
        updatePeakHoursChart(data.peakHours);
        
        // Update Daily Pattern Chart
        updateDailyPatternChart(data.dailyPattern);
        
        // Update Top Workers Table
        updateTopWorkersTable(data.topWorkers);
        
    } catch (err) {
        console.error("Analytics error:", err);
    }
}

// ═══════════════════════════════════════════════════════════════
// DAY OF WEEK CHART
// ═══════════════════════════════════════════════════════════════

function updateDayOfWeekChart(dayStats) {
    const labels = dayStats.map(d => d.dayOfWeek);
    const hours = dayStats.map(d => d.totalHours);
    
    if (dayOfWeekChart) {
        dayOfWeekChart.destroy();
    }
    
    const ctx = document.getElementById('dayOfWeekChart').getContext('2d');
    dayOfWeekChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Total Hours',
                data: hours,
                backgroundColor: [
                    '#667eea',
                    '#764ba2',
                    '#f093fb',
                    '#4facfe',
                    '#00f2fe',
                    '#43e97b',
                    '#38f9d7'
                ],
                borderRadius: 8
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Hours'
                    }
                }
            }
        }
    });
}

// ═══════════════════════════════════════════════════════════════
// PEAK HOURS CHART
// ═══════════════════════════════════════════════════════════════

function updatePeakHoursChart(peakHours) {
    const labels = peakHours.map(h => h.hourDisplay);
    const counts = peakHours.map(h => h.clockInsCount);
    
    if (peakHoursChart) {
        peakHoursChart.destroy();
    }
    
    const ctx = document.getElementById('peakHoursChart').getContext('2d');
    peakHoursChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                label: 'Clock-Ins',
                data: counts,
                backgroundColor: [
                    '#667eea',
                    '#764ba2',
                    '#f093fb',
                    '#4facfe',
                    '#00f2fe'
                ],
                borderWidth: 2,
                borderColor: '#fff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'right'
                }
            }
        }
    });
}

// ═══════════════════════════════════════════════════════════════
// DAILY PATTERN CHART
// ═══════════════════════════════════════════════════════════════

function updateDailyPatternChart(dailyPattern) {
    const labels = dailyPattern.map(d => {
        const date = new Date(d.date);
        return date.getDate(); // Just show day number (1, 2, 3...)
    });
    const hours = dailyPattern.map(d => d.totalHours);
    const workers = dailyPattern.map(d => d.workersActive);
    
    if (dailyPatternChart) {
        dailyPatternChart.destroy();
    }
    
    const ctx = document.getElementById('dailyPatternChart').getContext('2d');
    dailyPatternChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Total Hours',
                    data: hours,
                    borderColor: '#667eea',
                    backgroundColor: 'rgba(102, 126, 234, 0.1)',
                    tension: 0.4,
                    fill: true
                },
                {
                    label: 'Workers Active',
                    data: workers,
                    borderColor: '#43e97b',
                    backgroundColor: 'rgba(67, 233, 123, 0.1)',
                    tension: 0.4,
                    fill: true
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top',
                }
            },
            scales: {
                x: {
                    title: {
                        display: true,
                        text: 'Day of Month'
                    }
                },
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Count'
                    }
                }
            }
        }
    });
}

// ═══════════════════════════════════════════════════════════════
// TOP WORKERS TABLE
// ═══════════════════════════════════════════════════════════════

function updateTopWorkersTable(topWorkers) {
    const tbody = document.getElementById("topWorkersBody");
    
    if (!topWorkers || topWorkers.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" class="loading">No data for selected period</td></tr>';
        return;
    }
    
    tbody.innerHTML = topWorkers.map((worker, index) => {
        const medal = index === 0 ? '🥇' : index === 1 ? '🥈' : index === 2 ? '🥉' : '';
        
        return `
            <tr>
                <td><strong>${medal} ${index + 1}</strong></td>
                <td><strong>${worker.username}</strong></td>
                <td>${worker.totalHours}h</td>
                <td>${worker.daysWorked}</td>
                <td>${worker.averageHoursPerDay}h</td>
            </tr>
        `;
    }).join('');
}

// ═══════════════════════════════════════════════════════════════
// INITIALIZATION
// ═══════════════════════════════════════════════════════════════

document.addEventListener("DOMContentLoaded", () => {
    const user = checkAuth();
    if (!user) return;
    
    document.getElementById("adminName").innerText = `Admin: ${user.username}`;
    
    // Set current month and year
    const now = new Date();
    document.getElementById("monthSelect").value = now.getMonth() + 1;
    document.getElementById("yearSelect").value = now.getFullYear();
    
    // Load all data
    loadTrends();
    loadAnalytics();
});