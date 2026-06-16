let currentUser = { username: "", role: "Guest" };
let currentLayer = "surface";
let selectedSector = null;
async function checkAuth() {
    try {
        const response = await fetch('/api/auth/me');
        const data = await response.json();

        currentUser.role = data.role;
        currentUser.username = data.username || "";

        renderAuthBar();
        applyRolePermissions();
    } catch (error) {
        console.error("Ошибка проверки авторизации:", error);
    }
}

function renderAuthBar() {
    const authBar = document.getElementById('auth-bar');
    if (!authBar) return;

    if (currentUser.role === "Guest") {
        authBar.innerHTML = `
            <input type="text" id="login-username" placeholder="Логин">
            <input type="password" id="login-password" placeholder="Пароль">
            <button onclick="handleLogin()">Войти</button>
            <button onclick="handleRegister()" style="background: #2b579a;">Регистрация</button>
        `;
    } else {
        const roleName = currentUser.role === "Admin" ? "Гейм-Мастер" : "Игрок";
        authBar.innerHTML = `
            <span class="user-info">Привет, ${currentUser.username}! 
                <span class="role-badge">${roleName}</span>
            </span>
            <button onclick="handleLogout()" style="background: #a20025;">Выйти</button>
        `;
    }
}

function applyRolePermissions() {
    const adminPanel = document.getElementById('admin-panel');
    if (!adminPanel) return;

    if (currentUser.role === "Admin") {
        adminPanel.style.display = "block";
    } else {
        adminPanel.style.display = "none";
    }
}

async function handleLogin() {
    const usernameInput = document.getElementById('login-username').value;
    const passwordInput = document.getElementById('login-password').value;

    if (!usernameInput || !passwordInput) return alert("Заполните поля!");

    const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username: usernameInput, passwordHash: passwordInput })
    });

    if (response.ok) {
        checkAuth();
    } else {
        alert("Неверный логин или пароль!");
    }
}

async function handleRegister() {
    const usernameInput = document.getElementById('login-username').value;
    const passwordInput = document.getElementById('login-password').value;

    if (!usernameInput || !passwordInput) return alert("Заполните поля!");

    const response = await fetch('/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username: usernameInput, passwordHash: passwordInput })
    });

    if (response.ok) {
        alert("Регистрация успешна! Теперь нажмите кнопку 'Войти'.");
    } else {
        const errText = await response.text();
        alert(errText);
    }
}

async function handleLogout() {
    await fetch('/api/auth/logout', { method: 'POST' });
    checkAuth();
}

let allSectors = [];

function getFactionData(factionId) {
    switch (factionId) {
        case 1:
            return { name: "Кровавые Ангелы", color: "rgba(200, 30, 30, 0.55)" };
        case 2:
            return { name: "Некроны", color: "rgba(163, 240, 132, 0.55)" };
        case 3:
            return { name: "Аэлдари", color: "rgba(30, 100, 200, 0.55)" };
        case 4:
            return { name: "Астра Милитарум", color: "rgba(3, 76, 3, 0.55)" };
        default:
            return { name: "Нейтральная территория", color: "rgba(100, 100, 100, 0.4)" };
    }
}

function setMapLayer(layer) {
    currentLayer = layer;

    const btnSurface = document.getElementById('btn-layer-surface');
    const btnUnderground = document.getElementById('btn-layer-underground');
    const layerTitle = document.getElementById('current-layer-title');

    if (layer === 'surface') {
        if (btnSurface) { btnSurface.style.background = "#ff9800"; btnSurface.style.color = "#000"; }
        if (btnUnderground) { btnUnderground.style.background = "#222"; btnUnderground.style.color = "#888"; btnUnderground.style.border = "1px solid #444"; }
        if (layerTitle) layerTitle.innerText = "Поверхность";
    } else {
        if (btnUnderground) { btnUnderground.style.background = "#4db8ff"; btnUnderground.style.color = "#000"; }
        if (btnSurface) { btnSurface.style.background = "#222"; btnSurface.style.color = "#888"; btnSurface.style.border = "1px solid #444"; }
        if (layerTitle) layerTitle.innerText = "Подземные бункеры";
    }

    renderMapLayout();
}

async function loadMap() {
    try {
        const response = await fetch('/api/map');
        allSectors = await response.json();
        renderMapLayout();
    } catch (error) {
        console.error("Ошибка при получении карты:", error);
        document.getElementById('map-container').innerHTML = "<p style='color:red;'>Не удалось загрузить карту.</p>";
    }
}

function renderMapLayout() {
    if (!allSectors || allSectors.length === 0) return;

    let isUnder = (currentLayer === 'underground');

    let svgHtml = `
        <svg viewBox="0 0 800 600" width="100%" style="background: #0f0f11; border-radius: 8px; border: 2px solid ${isUnder ? '#4db8ff' : '#444'};">
    `;

    if (!isUnder) {
        svgHtml += `<image href="map.jpg" x="0" y="0" width="800" height="600" opacity="0.65" />`;
    } else {
        svgHtml += `<image href="underground.jpg" x="0" y="0" width="800" height="600" opacity="0.5" />`;
    }

    let gridColor = isUnder ? "rgba(77,184,255,0.08)" : "rgba(255,255,255,0.05)";
    svgHtml += `
        <g stroke="${gridColor}" stroke-width="1">
            <path d="M 0,100 L 800,100 M 0,200 L 800,200 M 0,300 L 800,300 M 0,400 L 800,400 M 0,500 L 800,500" />
            <path d="M 100,0 L 100,600 M 200,0 L 200,600 M 300,0 L 300,600 M 400,0 L 400,600 M 500,0 L 500,600 M 600,0 L 600,600 M 700,0 L 700,600" />
        </g>
    `;

    allSectors.forEach(sector => {
        let sectorIsUnderground = (sector.id >= 9 || sector.isUnderground === true);

        if (isUnder === sectorIsUnderground) {
            const faction = getFactionData(sector.controllingFactionId);
            let coords = sector.coordinates;

            if (!coords || coords.trim() === "") {
                coords = getDefaultCoords(sector.id);
            }

            svgHtml += `
                <g>
                    <polygon 
                        points="${coords}" 
                        fill="${faction.color}" 
                        fill-opacity="0.4" 
                        stroke="${isUnder ? '#4db8ff' : '#ff9800'}" 
                        stroke-width="${isUnder ? '1.5' : '1.2'}"
                        style="cursor: pointer; transition: fill-opacity 0.2s;"
                        onmouseover="this.setAttribute('fill-opacity', '0.6')"
                        onmouseout="this.setAttribute('fill-opacity', '0.4')"
                        onclick="selectSector(${sector.id})"
                    />
                    <text x="${getCenterOfCoords(coords, sector.id).x}" y="${getCenterOfCoords(coords).y}" fill="#fff" font-size="11" font-weight="bold" text-anchor="middle" style="pointer-events: none; text-shadow: 1px 1px 3px #000;">
                        ${sector.name}
                    </text>
                </g>
            `;
        }
    });

    svgHtml += `</svg>`;
    document.getElementById('map-container').innerHTML = svgHtml;
}

function getDefaultCoords(id) {
    if (id === 1) return "0,0 200,0 200,300 0,300";
    if (id === 2) return "200,0 400,0 400,300 200,300";
    if (id === 3) return "400,0 600,0 600,300 400,300";
    if (id === 4) return "600,0 800,0 800,300 600,300";
    if (id === 5) return "0,300 200,300 200,600 0,600";
    if (id === 6) return "200,300 400,300 400,600 200,600";
    if (id === 7) return "400,300 600,300 600,600 400,600";
    if (id === 8) return "600,300 800,300 800,600 600,600";

    if (id === 9) return "0,0 266,0 266,300 0,300";
    if (id === 10) return "266,0 533,0 533,300 266,300";
    if (id === 11) return "533,0 800,0 800,300 533,300";
    if (id === 12) return "0,300 266,300 266,600 0,600";
    if (id === 13) return "266,300 533,300 533,600 266,600";
    if (id === 14) return "533,300 800,300 800,600 533,600";

    return "0,0 100,0 100,100 0,100";
}

function getCenterOfCoords(coordsStr, sectorId) {
    if (sectorId === 14) {
        return { x: 700, y: 460 };
    }
    try {
        let pairs = coordsStr.split(' ');
        let sumX = 0, sumY = 0, count = 0;
        pairs.forEach(p => {
            let parts = p.split(',');
            if (parts.length === 2) {
                sumX += parseFloat(parts[0]);
                sumY += parseFloat(parts[1]);
                count++;
            }
        });
        return { x: sumX / count, y: sumY / count };
    } catch (e) {
        return { x: 400, y: 300 };
    }
}

let selectedSectorId = null;

function addAdminFileRow(name = '', url = '') {
    const container = document.getElementById('admin-files-container');
    if (!container) return;

    const row = document.createElement('div');
    row.className = 'admin-file-row';
    row.style = 'display: flex; gap: 10px; align-items: center; width: 100%; box-sizing: border-box;';
    row.innerHTML = `
        <input type="text" class="admin-input file-name" placeholder="Название файла (например: Карта высот)" value="${name}" style="flex: 1;">
        <input type="text" class="admin-input file-url" placeholder="Ссылка на документ" value="${url}" style="flex: 2;">
        <button type="button" onclick="this.parentElement.remove()" style="padding: 12px; background: rgba(162,0,37,0.2); border: 1px solid #a20025; color: #ff4d4d; border-radius:4px; cursor:pointer; font-weight:bold;">✕</button>
    `;
    container.appendChild(row);
}

function getAdminFilesData() {
    const rows = document.querySelectorAll('.admin-file-row');
    const files = [];
    rows.forEach(row => {
        const name = row.querySelector('.file-name').value.trim();
        const url = row.querySelector('.file-url').value.trim();
        if (name && url) {
            files.push({ name, url });
        }
    });
    return files;
}

function selectSector(sectorId) {
    selectedSector = allSectors.find(s => s.id === sectorId);
    if (!selectedSector) {
        console.error("Сектор не найден!");
        return;
    }

    const sector = selectedSector;
    document.getElementById('admin-sector-name-label').innerText = sector.name;

    const faction = getFactionData(sector.controllingFactionId);
    const container = document.getElementById('sector-missions-container');
    if (!container) return;

    const missionName = sector.missionName || "Информация о секторе";
    const missionStatus = sector.missionStatus || "active";
    const descriptionText = sector.description || "Особых условий правил миссии и ландшафта не заявлено.";
    const currentMarks = sector.gmMarks || "";
    const files = sector.files || [];

    const hasVoted = sector.hasVoted;

    if (missionStatus === "hidden" && currentUser.role !== "Admin") {
        container.innerHTML = `
            <div style="text-align: center; padding: 40px 20px; color: #666;">
                <p style="font-style: italic; margin: 0;">В данном секторе нет активных боевых операций. Высадка недоступна.</p>
            </div>
        `;
    } else {
        let filesHtml = "";
        if (files.length > 0) {
            filesHtml = `
                <div style="margin-top: 15px; border-top: 1px solid #333; padding-top: 12px;">
                    <strong style="color: #fff; font-size: 0.9rem; display: block; margin-bottom: 8px;">📁 Материалы брифинга:</strong>
                    <div style="display: flex; flex-direction: column; gap: 6px;">
                        ${files.map(f => `
                            <a href="${f.url}" target="_blank" style="display: flex; align-items: center; background: #1c2024; border: 1px solid #444; color: #4db8ff; padding: 8px 12px; border-radius: 4px; text-decoration: none; font-size: 0.85rem; font-weight: bold; transition: 0.2s;" onmouseover="this.style.borderColor='#4db8ff'" onmouseout="this.style.borderColor='#444'">
                                <span style="margin-right: 6px;">📄</span> ${f.name}
                            </a>
                        `).join('')}
                    </div>
                </div>
            `;
        }

        let voteButtonHtml = "";
        if (currentUser.role === "Player") {
            voteButtonHtml = `
                <div style="margin-top: 20px; border-top: 1px dashed #444; padding-top: 15px;">
                    ${hasVoted
                    ? `<div style="background: rgba(76,175,80,0.1); border: 1px solid #4caf50; padding: 10px; border-radius: 4px; color: #4caf50; font-weight: bold; text-align: center; font-size: 0.9rem;">
                            Ваша готовность к участию в этой миссии подтверждена
                       </div>`
                    : `<button onclick="voteForSector(${sectorId})" style="width: 100%; background: #2b579a; color: white; border: none; padding: 12px; border-radius: 4px; cursor: pointer; font-weight: bold; font-size: 0.9rem; text-transform: uppercase; letter-spacing: 1px; transition: background 0.2s; box-shadow: 0 4px 10px rgba(43,87,154,0.3);" onmouseover="this.style.background='#3b6cb3'" onmouseout="this.style.background='#2b579a'">
                            Голосовать за участие в миссии
                       </button>`
                }
                </div>
            `;
        }

        let archiveStatusHtml = (missionStatus === "completed")
            ? `<div style="background: rgba(162,0,37,0.15); border: 1px solid #a20025; color: #ff4d4d; padding: 6px; text-align: center; border-radius: 4px; font-weight: bold; font-size: 0.8rem; text-transform: uppercase; margin-bottom: 15px;">🔒 Миссия завершена (Архив)</div>`
            : "";

        container.innerHTML = `
            ${archiveStatusHtml}
            <div style="background: #1c1f22; border: 1px solid #3a3f44; padding: 15px; border-radius: 6px; box-shadow: 0 4px 10px rgba(0,0,0,0.4);">
                <div style="display: flex; justify-content: space-between; align-items: flex-start; border-bottom: 1px dashed #444; padding-bottom: 8px; margin-bottom: 12px;">
                    <h3 style="margin: 0; color: #ff9800; font-family: 'Courier New', monospace; font-size: 1.15rem;">${missionName}</h3>
                    <span style="font-size: 0.75rem; color: #555; font-family: monospace;">Сектор ${sectorId}</span>
                </div>
                <div style="margin-bottom: 12px;">
                    <span style="font-size: 0.75rem; color: #888; text-transform: uppercase; display:block;">Контроль сектора:</span>
                    <span class="faction-badge" style="background-color: ${faction.color}; color: #fff; padding: 3px 8px; border-radius: 4px; display: inline-block; font-weight: bold; margin-top: 4px; font-size: 0.85rem; border: 1px solid rgba(255,255,255,0.1);">
                        ${faction.name}
                    </span>
                </div>
                <div style="color: #ddd; font-size: 0.9rem; line-height: 1.4;">
                    <strong style="color: #aaa; font-size: 0.8rem; text-transform: uppercase; display:block; margin-bottom: 4px;">Правила миссии и лор:</strong>
                    <p style="margin: 0; white-space: pre-line;">${descriptionText}</p>
                </div>
                ${currentMarks.trim() !== "" ? `
                    <div style="margin-top: 12px; padding: 8px 10px; background: rgba(77, 184, 255, 0.08); border-left: 3px solid #4db8ff; color: #4db8ff; font-size: 0.85rem; border-radius: 0 4px 4px 0;">
                        <strong>Фронтовые разведданные:</strong> ${currentMarks.replace(/Операция утверждена силами: \[[^\]]+\](,\s*\[[^\]]+\])*/g, '').trim()}
                    </div>
                ` : ""}
                ${filesHtml}
                ${missionStatus === "active" ? voteButtonHtml : ""}
            </div>
        `;
    }

    const adminSectorIdInput = document.getElementById('admin-sector-id');
    const adminSectorNameLabel = document.getElementById('admin-sector-name-label');
    const adminFactionSelect = document.getElementById('admin-faction-select');
    const adminMissionStatus = document.getElementById('admin-mission-status');
    const adminMissionName = document.getElementById('admin-mission-name');
    const adminDesc = document.getElementById('admin-sector-desc');
    const adminMarks = document.getElementById('admin-sector-marks');
    const adminFilesContainer = document.getElementById('admin-files-container');
    const adminVotesLog = document.getElementById('admin-votes-log');

    if (adminSectorIdInput) adminSectorIdInput.value = sectorId;
    if (adminSectorNameLabel) adminSectorNameLabel.innerText = `${sector.name} (ID: ${sectorId})`;
    if (adminFactionSelect) adminFactionSelect.value = sector.controllingFactionId || 0;
    if (adminMissionStatus) adminMissionStatus.value = missionStatus;
    if (adminMissionName) adminMissionName.value = sector.missionName || "";
    if (adminDesc) adminDesc.value = sector.description || "";
    if (adminMarks) adminMarks.value = currentMarks;

    if (adminFilesContainer) {
        adminFilesContainer.innerHTML = '';
        files.forEach(f => addAdminFileRow(f.name, f.url));
    }

    if (adminVotesLog) {
        const voters = JSON.parse(selectedSector.voterList || "[]");
        adminVotesLog.innerHTML = voters.length > 0
            ? `<span>Проголосовали (${voters.length}):</span>` + voters.map(v => `<span> ${v}</span>`).join('')
            : "Заявок нет.";
    }
}


async function voteForSector(sectorId) {
    if (currentUser.role !== "Player") {
        alert("Голосовать могут только авторизованные игроки!");
        return;
    }

    try {
        const response = await fetch(`/api/map/${sectorId}/vote`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });

        if (response.ok) {
            const mapResp = await fetch('/api/map');

            if (mapResp.ok) {
                allSectors = await mapResp.json();

                selectSector(parseInt(sectorId, 10));
                alert("Ваша готовность к участию в миссии подтверждена!");
            }
        } else {
            const err = await response.text();
            alert("Ошибка при голосовании: " + err);
        }
    } catch (e) {
        console.error("Ошибка сети:", e);
    }
}


async function saveSectorChanges() {
    const currentSectorId = document.getElementById('admin-sector-id').value;
    if (!currentSectorId) {
        alert("Сначала выберите сектор на стратегической карте!");
        return;
    }

    const factionSelectValue = document.getElementById('admin-faction-select').value;
    const mStatus = document.getElementById('admin-mission-status').value;
    const mName = document.getElementById('admin-mission-name').value.trim();
    const newMarks = document.getElementById('admin-sector-marks').value.trim();
    const newDesc = document.getElementById('admin-sector-desc').value.trim();

    const newFactionId = parseInt(factionSelectValue, 10);
    const updatedFiles = getAdminFilesData();


    const currentSector = allSectors.find(s => s.id === parseInt(currentSectorId, 10));
    const originalSectorName = currentSector ? currentSector.name : "Сектор";


    const updatedData = {
        name: originalSectorName,
        controllingFactionId: isNaN(newFactionId) ? 0 : newFactionId,
        missionStatus: mStatus,
        missionName: mName || "Разведывательная миссия",
        description: newDesc,
        gmMarks: newMarks,
        files: updatedFiles
    };

    try {
        const response = await fetch(`/api/map/${currentSectorId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(updatedData)
        });

        if (response.ok) {
            alert("Параметры операции и файлы брифинга успешно обновлены в базе данных!");

            const mapResp = await fetch('/api/map');
            allSectors = await mapResp.json();
            selectSector(parseInt(currentSectorId, 10));
        } else {

            const errDetails = await response.text();
            console.error("Детали ошибки 400 от сервера:", errDetails);
            alert(`Сервер отклонил сохранение данных сектора (400): ${errDetails}`);
        }
    } catch (e) {
        console.error("Ошибка сохранения сектора:", e);
        alert("Сбой сети при отправке пакета изменений.");
    }
}

document.addEventListener("DOMContentLoaded", () => {
    loadMap();
    checkAuth();
});