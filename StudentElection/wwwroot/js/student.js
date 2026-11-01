const $ = id => document.getElementById(id);
let voter = null;
let selections = {};
let required = [];
let timer = null;

async function post(url, body) {
    const res = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });
    let data = {};
    try { data = await res.json(); } catch { }
    return { ok: res.ok, data };
}

function show(step) {
    ['step1', 'step2', 'step3'].forEach(s => $(s).classList.toggle('hidden', s !== step));
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function updateButton() {
    $('btnVote').disabled = required.length === 0 ||
        !required.every(k => (selections[k] || []).length > 0);
}

function buildBallot(positions) {
    const box = $('ballot');
    box.innerHTML = '';
    selections = {};
    required = [];

    positions.forEach((p, i) => {
        const q = document.createElement('div');
        q.className = 'q';

        const label = document.createElement('div');
        label.className = 'qlabel';
        label.textContent = `${i + 1}. ${p.label} `;
        q.appendChild(label);

        if (p.candidates.length === 0) {
            const none = document.createElement('div');
            none.className = 'none';
            none.textContent = 'No candidates yet.';
            q.appendChild(none);
            box.appendChild(q);
            return;
        }

        const star = document.createElement('span');
        star.className = 'req';
        star.textContent = '*';
        label.appendChild(star);
        required.push(p.key);

        if (p.maxPicks > 1) {
            const hint = document.createElement('span');
            hint.className = 'hint';
            hint.textContent = `(Select up to ${p.maxPicks})`;
            label.appendChild(hint);

            const wrap = document.createElement('div');
            wrap.className = 'opts';
            const items = [];

            p.candidates.forEach(c => {
                const row = document.createElement('label');
                row.className = 'opt';
                const cb = document.createElement('input');
                cb.type = 'checkbox';
                cb.value = c.id;
                const span = document.createElement('span');
                span.textContent = c.name;
                row.appendChild(cb);
                row.appendChild(span);
                wrap.appendChild(row);
                items.push({ row, cb });

                cb.onchange = () => {
                    const ids = items.filter(x => x.cb.checked).map(x => parseInt(x.cb.value));
                    if (ids.length) selections[p.key] = ids; else delete selections[p.key];
                    const full = ids.length >= p.maxPicks;
                    items.forEach(x => {
                        x.row.classList.toggle('checked', x.cb.checked);
                        const lock = full && !x.cb.checked;
                        x.cb.disabled = lock;
                        x.row.classList.toggle('disabled', lock);
                    });
                    updateButton();
                };
            });
            q.appendChild(wrap);
        } else {
            const sel = document.createElement('select');
            sel.innerHTML = '<option value="">Select your answer</option>';
            p.candidates.forEach(c => {
                const o = document.createElement('option');
                o.value = c.id;
                o.textContent = c.name;
                sel.appendChild(o);
            });
            sel.onchange = () => {
                if (sel.value) selections[p.key] = [parseInt(sel.value)];
                else delete selections[p.key];
                updateButton();
            };
            q.appendChild(sel);
        }
        box.appendChild(q);
    });
    updateButton();
}

function fillInfo() {
    const box = $('infoBox');
    box.innerHTML = '';
    [['Name', voter.name.trim()], ['Voter ID', voter.voterIdNo.trim()], ['Grade Level', 'Grade ' + voter.grade]]
        .forEach(([k, v]) => {
            const d = document.createElement('div');
            const b = document.createElement('b');
            b.textContent = k + ': ';
            d.appendChild(b);
            d.appendChild(document.createTextNode(v));
            box.appendChild(d);
        });
}

$('btnSubmit').onclick = async () => {
    $('err1').textContent = '';
    voter = { name: $('name').value, voterIdNo: $('vid').value, grade: parseInt($('grade').value || '0') };
    if (!voter.name.trim() || !voter.voterIdNo.trim() || !voter.grade) {
        $('err1').textContent = 'Please fill in all fields.';
        return;
    }
    const { ok, data } = await post('/api/student/check', voter);
    if (!ok) { $('err1').textContent = data.message || 'Something went wrong.'; return; }

    fillInfo();
    buildBallot(data.positions);
    $('err2').textContent = '';
    show('step2');
};

$('btnBack').onclick = () => {
    $('err1').textContent = '';
    show('step1');
};

$('btnVote').onclick = async () => {
    $('err2').textContent = '';
    $('btnVote').disabled = true;
    const { ok, data } = await post('/api/student/vote', { ...voter, selections });
    if (!ok) {
        $('err2').textContent = data.message || 'Something went wrong.';
        $('btnVote').disabled = false;
        return;
    }
    show('step3');
    startCountdown();
};

function startCountdown() {
    let n = 5;
    const btn = $('btnRestart');
    btn.textContent = `Back to Start (${n})`;
    clearInterval(timer);
    timer = setInterval(() => {
        n--;
        if (n <= 0) goStart();
        else btn.textContent = `Back to Start (${n})`;
    }, 1000);
}

function goStart() {
    clearInterval(timer);
    voter = null;
    selections = {};
    required = [];
    $('name').value = '';
    $('vid').value = '';
    $('grade').value = '';
    $('err1').textContent = '';
    $('err2').textContent = '';
    show('step1');
}

$('btnRestart').onclick = goStart;

