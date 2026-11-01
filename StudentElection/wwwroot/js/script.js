const $ = id => document.getElementById(id);
let key = '';
try { key = sessionStorage.getItem('adminKey') || ''; } catch (e) { }
let positions = [];
let charts = [];
const PALETTE = ['#e10b1c', '#1f3f9e', '#e8b930', '#a30612', '#3b6fe0', '#b8891a'];

function api(url, opt = {}) {
    opt.headers = { ...(opt.headers || {}), 'X-Admin-Key': key };
    return fetch(url, opt);
}

$('btnLogin').onclick = async () => {
    const pw = $('pw').value;
    const res = await fetch('/api/admin/login', {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ password: pw })
    });
    if (!res.ok) { $('loginErr').textContent = 'Incorrect password.'; return; }
    key = pw;
    try { sessionStorage.setItem('adminKey', pw); } catch (e) { }
    showApp();
};
$('pw').addEventListener('keydown', e => { if (e.key === 'Enter') $('btnLogin').click(); });

async function showApp() {
    const res = await api('/api/admin/positions');
    if (res.status === 401) return logout();
    positions = await res.json();

    $('cPosition').innerHTML = positions.map(p =>
        `<option value="${esc(p.key)}">${esc(p.label)}</option>`).join('');
    toggleGrade();

    $('login-wrap').classList.add('hidden');
    document.documentElement.classList.remove('has-key');
    $('login-wrap').style.display = 'none';
    $('app').classList.remove('hidden');
    window.scrollTo(0, 0);

    refresh();
}

$('cPosition').onchange = toggleGrade;
function toggleGrade() {
    $('cGrade').classList.toggle('hidden', $('cPosition').value !== 'Representative');
}

function logout() {
    try { sessionStorage.removeItem('adminKey'); } catch (e) { }
    location.reload();
}

async function refresh() {
    await Promise.all([loadCandidates(), loadResults(), loadVotes()]);
}

async function loadCandidates() {
    const res = await api('/api/admin/candidates');
    if (res.status === 401) return logout();
    const data = await res.json();
    $('candBody').innerHTML = data.map(c => `
      <tr><td>${esc(c.name)}</td>
      <td>${esc(c.positionLabel)}${c.grade ? ' - Grade ' + c.grade : ''}</td>
      <td><button class="red small" onclick="delCandidate(${c.id})">Delete</button></td></tr>`).join('')
        || '<tr><td colspan="3">No candidates yet.</td></tr>';
}

async function addCandidate() {
    $('cErr').textContent = '';
    const position = $('cPosition').value;
    const res = await api('/api/admin/candidates', {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            name: $('cName').value,
            position,
            grade: position === 'Representative' ? parseInt($('cGrade').value) : null
        })
    });
    if (!res.ok) { const d = await res.json(); $('cErr').textContent = d.message || 'Error'; return; }
    $('cName').value = '';
    refresh();
}

async function delCandidate(id) {
    if (!confirm('Delete this candidate?')) return;
    const res = await api('/api/admin/candidates/' + id, { method: 'DELETE' });
    if (!res.ok) { const d = await res.json(); alert(d.message || 'Error'); return; }
    refresh();
}

async function loadResults() {
    const res = await api('/api/admin/results');
    const data = await res.json();

    charts.forEach(c => c.destroy());
    charts = [];
    const box = $('charts');
    box.innerHTML = '';

    positions.forEach(p => {
        const list = data.filter(d => d.position === p.key);
        if (!list.length) return;

        const div = document.createElement('div');
        div.className = 'chartbox';
        div.innerHTML = '<h3></h3><canvas></canvas>';
        div.querySelector('h3').textContent = p.label;
        box.appendChild(div);

        charts.push(new Chart(div.querySelector('canvas'), {
            type: 'bar',
            data: {
                labels: list.map(d => p.key === 'Representative' ? `${d.name} (G${d.grade})` : d.name),
                datasets: [{
                    label: 'Votes',
                    data: list.map(d => d.votes),
                    backgroundColor: list.map((_, i) => PALETTE[i % PALETTE.length]),
                    borderRadius: 6
                }]
            },
            options: {
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
            }
        }));
    });

    if (!box.children.length) box.innerHTML = '<p>No candidates yet.</p>';
}

async function loadVotes() {
    const res = await api('/api/admin/votes');
    const data = await res.json();
    $('total').textContent = 'Total voters: ' + data.length;

    $('voteHead').innerHTML =
        '<th>#</th><th>Voter</th><th>Voter ID</th><th>Grade</th>' +
        positions.map(p => `<th>${esc(p.label)}</th>`).join('') +
        '<th>Date/Time</th>';

    $('voteBody').innerHTML = data.map(v => `
      <tr><td>${v.id}</td><td>${esc(v.voterName)}</td><td>${esc(v.voterIdNo)}</td><td>G${v.voterGrade}</td>
      ${positions.map(p => `<td>${esc(v.choices[p.key] ?? '-')}</td>`).join('')}
      <td>${new Date(v.votedAt).toLocaleString()}</td></tr>`).join('')
        || `<tr><td colspan="${positions.length + 5}">No votes yet.</td></tr>`;
}

async function exportExcel() {
    const res = await api('/api/admin/export');
    if (!res.ok) { alert('Export failed.'); return; }
    const blob = await res.blob();
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = `sslg-election-${new Date().toISOString().slice(0, 10)}.xlsx`;
    a.click();
}

async function resetData(mode) {
    const msg = mode === 'all'
        ? 'This will permanently delete ALL votes AND ALL candidates.'
        : 'This will permanently delete ALL votes. Candidates will be kept.';
    const t = prompt(msg + '\n\nTip: click "Export Excel" first to keep a copy.\n\nType RESET to continue:');
    if (t !== 'RESET') return;

    const res = await api('/api/admin/reset', {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ mode })
    });
    if (!res.ok) { const d = await res.json().catch(() => ({})); alert(d.message || 'Reset failed.'); return; }
    alert('Reset complete.');
    refresh();
}

function esc(s) { return String(s).replace(/[&<>"']/g, m => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }
    [m])); 
}

if (key) showApp();

