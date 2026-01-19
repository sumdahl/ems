"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/notificationHub").build();

// Start the connection
connection.start().then(function () {
    console.log("SignalR Connected!");
}).catch(function (err) {
    return console.error(err.toString());
});

// Listen for messages
connection.on("ReceiveNotification", function (message) {
    showToast(message);
});

// Listen for system updates
connection.on("ReceiveSystemUpdate", function (updateType) {
    if (updateType === "LeaveRequests" || updateType === "Attendance") {
        updateNotificationCounts();
    }
});

function updateNotificationCounts() {
    fetch('/api/Notification/counts')
        .then(response => response.json())
        .then(data => {
            const leaveBadge = document.getElementById('leaveRequestBadge');
            if (leaveBadge) {
                leaveBadge.innerText = data.leaveRequests;
                if (data.leaveRequests > 0) {
                    leaveBadge.classList.remove('hidden');
                } else {
                    leaveBadge.classList.add('hidden');
                }
            }

            const attendanceBadge = document.getElementById('attendanceBadge');
            if (attendanceBadge) {
                attendanceBadge.innerText = data.attendance;
                if (data.attendance > 0) {
                    attendanceBadge.classList.remove('hidden');
                } else {
                    attendanceBadge.classList.add('hidden');
                }
            }
        })
        .catch(err => console.error("Error updating notification counts:", err));
}

// Initial check on load
document.addEventListener('DOMContentLoaded', () => {
    // Optional: We could fetch initial counts here too, but Razor handles the initial render.
});

function showToast(message) {
    // Create toast container if it doesn't exist
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'fixed bottom-5 right-5 z-50 flex flex-col space-y-2';
        document.body.appendChild(toastContainer);
    }

    // Create toast element
    const toast = document.createElement('div');
    toast.className = 'bg-white border-l-4 border-indigo-500 text-gray-700 p-4 rounded shadow-lg flex items-center justify-between min-w-[300px] transform transition-all duration-300 ease-in-out translate-y-2 opacity-0';
    toast.innerHTML = `
        <div class="flex items-center">
            <svg class="w-6 h-6 text-indigo-500 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
            </svg>
            <span class="font-medium">${message}</span>
        </div>
        <button onclick="this.parentElement.remove()" class="text-gray-400 hover:text-gray-600 focus:outline-none">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
            </svg>
        </button>
    `;

    toastContainer.appendChild(toast);

    // Animate in
    requestAnimationFrame(() => {
        toast.classList.remove('translate-y-2', 'opacity-0');
    });

    // Auto remove after 5 seconds
    setTimeout(() => {
        toast.classList.add('opacity-0', 'translate-y-2');
        setTimeout(() => toast.remove(), 300);
    }, 5000);
}
