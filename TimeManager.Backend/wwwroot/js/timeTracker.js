function startLiveClock() {
    const tracker = document.getElementById('running-total');
    if (!tracker) return;

    const clockInTime = new Date(tracker.getAttribute('data-clockin'));

    function updateTimer() {
        const now = new Date();
        const differenceInMs = now - clockInTime;

        if (differenceInMs < 0) {
            tracker.textContent = "0h 00m";
            return;
        }

        const totalSeconds = Math.floor(differenceInMs / 1000);
        const hours = Math.floor(totalSeconds / 3600);
        const minutes = Math.floor((totalSeconds % 3600) / 60);

        const formattedMinutes = String(minutes).padStart(2, '0');

        tracker.textContent = `${hours}h ${formattedMinutes}m`;
    }

    updateTimer();
    setInterval(updateTimer, 1000);
}

document.addEventListener('DOMContentLoaded', startLiveClock);