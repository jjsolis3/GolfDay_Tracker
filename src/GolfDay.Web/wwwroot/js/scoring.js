// GolfDay Tracker - Live Scoring JavaScript (SignalR)

'use strict';

let scoringConnection = null;
let currentEventId = null;
let pendingScores = {};
let debounceTimers = {};

async function initScoringHub(eventId, roundId) {
    currentEventId = eventId;

    scoringConnection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/scoring')
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    // Handle reconnecting state
    scoringConnection.onreconnecting(function () {
        showConnectionStatus('warning', 'Reconnecting to live scoring...');
    });

    scoringConnection.onreconnected(function () {
        showConnectionStatus('success', 'Connected');
        scoringConnection.invoke('JoinEventGroup', eventId).catch(console.error);
    });

    scoringConnection.onclose(function () {
        showConnectionStatus('danger', 'Disconnected from live scoring');
    });

    // Handle incoming score update from another player
    scoringConnection.on('ScoreUpdated', function (data) {
        updateLeaderboardRow(data);
        showLiveNotification(`${data.playerName} — Hole ${data.holeNumber}: ${data.scoreLabel}`);
    });

    // Handle round completion notification
    scoringConnection.on('RoundCompleted', function (data) {
        const row = document.getElementById('leaderboard-row-' + data.roundId);
        if (row) {
            row.classList.add('table-success');
            const statusBadge = row.querySelector('.completion-status');
            if (statusBadge) statusBadge.textContent = 'F';
        }
    });

    try {
        await scoringConnection.start();
        showConnectionStatus('success', 'Live');
        await scoringConnection.invoke('JoinEventGroup', eventId);
    } catch (err) {
        showConnectionStatus('danger', 'Offline — scores saved locally');
        console.error('SignalR connection error:', err);
    }
}

function showConnectionStatus(type, message) {
    const statusEl = document.getElementById('connection-status');
    if (!statusEl) return;
    statusEl.className = `badge bg-${type}`;
    statusEl.textContent = message;
}

function updateLeaderboardRow(data) {
    // Find or create row by roundId
    const row = document.getElementById('leaderboard-row-' + data.roundId);
    if (!row) return;

    // Update holes completed
    const thruCell = row.querySelector('.col-thru');
    if (thruCell) thruCell.textContent = data.holesCompleted;

    // Update score
    const scoreCell = row.querySelector('.col-score');
    if (scoreCell) scoreCell.textContent = data.grossTotal ?? '-';

    // Re-sort leaderboard
    sortLeaderboard();
}

function sortLeaderboard() {
    const tbody = document.getElementById('leaderboard-body');
    if (!tbody) return;
    const rows = Array.from(tbody.querySelectorAll('tr'));
    rows.sort(function (a, b) {
        const aScore = parseInt(a.querySelector('.col-score')?.textContent) || 9999;
        const bScore = parseInt(b.querySelector('.col-score')?.textContent) || 9999;
        return aScore - bScore;
    });
    rows.forEach(function (row, i) {
        tbody.appendChild(row);
        const rankCell = row.querySelector('.col-rank');
        if (rankCell) rankCell.textContent = i + 1;
        row.className = i < 3 ? `position-${i + 1}` : '';
    });
}

function showLiveNotification(message) {
    const container = document.getElementById('live-notifications');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = 'toast align-items-center text-bg-success border-0 mb-2';
    toast.setAttribute('role', 'alert');
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body"><i class="bi bi-broadcast me-2"></i>${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>`;
    container.appendChild(toast);
    const bsToast = new bootstrap.Toast(toast, { delay: 4000 });
    bsToast.show();
    toast.addEventListener('hidden.bs.toast', function () { toast.remove(); });
}

// Called when a hole score input changes
function onHoleScoreChange(input, roundId) {
    const holeNumber = parseInt(input.dataset.hole);
    const par = parseInt(input.dataset.par);
    const strokes = parseInt(input.value);

    // Apply score color
    applyScoreColor(input);

    // Update running total
    updateRunningTotal();

    if (isNaN(strokes) || strokes <= 0 || !roundId) return;

    // Debounce sending to hub (300ms)
    if (debounceTimers[holeNumber]) clearTimeout(debounceTimers[holeNumber]);
    debounceTimers[holeNumber] = setTimeout(async function () {
        if (scoringConnection && scoringConnection.state === signalR.HubConnectionState.Connected) {
            try {
                await scoringConnection.invoke('PostHoleScore', roundId, holeNumber, strokes, null, null, null);
            } catch (err) {
                console.error('Error posting score:', err);
            }
        }
    }, 300);
}

function updateRunningTotal() {
    let total = 0;
    let parTotal = 0;
    let holesPlayed = 0;

    document.querySelectorAll('.hole-score-input').forEach(function (input) {
        const val = parseInt(input.value);
        const par = parseInt(input.dataset.par || 4);
        if (!isNaN(val) && val > 0) {
            total += val;
            parTotal += par;
            holesPlayed++;
        }
    });

    const totalEl = document.getElementById('score-total');
    const toParEl = document.getElementById('score-to-par');
    const thruEl = document.getElementById('score-thru');

    if (totalEl) totalEl.textContent = total || '-';
    if (thruEl) thruEl.textContent = holesPlayed;
    if (toParEl && holesPlayed > 0) {
        const diff = total - parTotal;
        toParEl.textContent = diff === 0 ? 'E' : (diff > 0 ? '+' + diff : diff.toString());
        toParEl.className = 'fw-bold fs-5 ' + (diff < 0 ? 'text-danger' : diff === 0 ? 'text-success' : 'text-secondary');
    }
}

async function completeRound(roundId) {
    if (!confirm('Are you sure you want to finalize this round? This cannot be undone.')) return;

    if (scoringConnection && scoringConnection.state === signalR.HubConnectionState.Connected) {
        try {
            await scoringConnection.invoke('CompleteRound', roundId);
            document.getElementById('complete-round-btn')?.setAttribute('disabled', 'true');
            showLiveNotification('Round completed! Final score submitted.');
        } catch (err) {
            console.error('Error completing round:', err);
        }
    }
}
