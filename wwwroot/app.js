// Текущий вошедший пользователь
let currentUser = { username: "", role: "Guest" };
// Текущий активный слой карты ("surface" - поверхность, "underground" - подземка)
let currentLayer = "surface";

// 1. Проверка авторизации при загрузке страницы
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
            <button onclick="handleRegister()" style="background: #2b579a;">Рег</button>
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

// Отрендерить сетку и полигоны
function renderMapLayout() {
    if (!allSectors || allSectors.length === 0) return;

    let isUnder = (currentLayer === 'underground');

    let svgHtml = `
        <svg viewBox="0 0 800 600" width="100%" height="auto" style="background: #0f0f11; border-radius: 8px; border: 2px solid ${isUnder ? '#4db8ff' : '#444'};">
    `;

    // Выбор фоновой картинки в зависимости от слоя
    if (!isUnder) {
        svgHtml += `<image href="map.jpg" x="0" y="0" width="800" height="600" opacity="0.65" />`;
    } else {
        svgHtml += `<image href="underground.jpg" x="0" y="0" width="800" height="600" opacity="0.5" />`;
    }

    // Технологическая сетка радара
    let gridColor = isUnder ? "rgba(77,184,255,0.08)" : "rgba(255,255,255,0.05)";
    svgHtml += `
        <g stroke="${gridColor}" stroke-width="1">
            <path d="M 0,100 L 800,100 M 0,200 L 800,200 M 0,300 L 800,300 M 0,400 L 800,400 M 0,500 L 800,500" />
            <path d="M 100,0 L 100,600 M 200,0 L 200,600 M 300,0 L 300,600 M 400,0 L 400,600 M 500,0 L 500,600 M 600,0 L 600,600 M 700,0 L 700,600" />
        </g>
    `;

    allSectors.forEach(sector => {
        // Сектора с ID >= 9 уходят под землю
        let sectorIsUnderground = (sector.id >= 9 || sector.isUnderground === true);

        if (isUnder === sectorIsUnderground) {
            const faction = getFactionData(sector.controllingFactionId);
            let coords = sector.coordinates;

            // Резервный расчет координат на случай сброса БД
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

// Дефолтная сетка, если в базе стерты координаты
function getDefaultCoords(id) {
    // Поверхность: 8 зон (2 ряда по 4 зоны)
    if (id === 1) return "0,0 200,0 200,300 0,300";
    if (id === 2) return "200,0 400,0 400,300 200,300";
    if (id === 3) return "400,0 600,0 600,300 400,300";
    if (id === 4) return "600,0 800,0 800,300 600,300";
    if (id === 5) return "0,300 200,300 200,600 0,600";
    if (id === 6) return "200,300 400,300 400,600 200,600";
    if (id === 7) return "400,300 600,300 600,600 400,600";
    if (id === 8) return "600,300 800,300 800,600 600,600";

    // Подземелье: 6 зон (2 ряда по 3 зоны)
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
        return { x: 700, y: 460 }; // Смещаем точку отрисовки текста вглубь правой части
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

function selectSector(sectorId) {
    selectedSectorId = sectorId;
    const sector = allSectors.find(s => s.id === sectorId);
    if (!sector) return;

    const faction = getFactionData(sector.controllingFactionId);
    const descriptionText = sector.description || "Особых примет ландшафта не зарегистрировано.";

    let marksHtml = "";
    if (sector.gmMarks && sector.gmMarks.trim() !== "") {
        marksHtml = `
            <div style="margin-top: 15px; padding: 10px; background: rgba(77, 184, 255, 0.1); border-left: 4px solid #4db8ff; color: #4db8ff;">
                <strong>Тактические разведданные:</strong><br>
                ${sector.gmMarks}
            </div>
        `;
    }

    const detailsContainer = document.getElementById('sector-details');
    if (detailsContainer) {
        detailsContainer.innerHTML = `
            <h3>${sector.name}</h3>
            <p><strong>Статус контроля:</strong></p>
            <span class="faction-badge" style="background-color: ${faction.color}; color: #fff; padding: 5px 10px; border-radius: 4px; display: inline-block; font-weight: bold; border: 1px solid rgba(255,255,255,0.2);">
                ${faction.name}
            </span>
            
            <p style="margin-top: 15px; line-height: 1.4; color: #ccc;">
                <strong>Сводка ландшафта:</strong><br>${descriptionText}
            </p>

            ${marksHtml}

            <p style="margin-top: 20px;"><small style="color: #555;">Системный ID: ${sector.id} | Тип: ${sector.isUnderground ? 'Подземелье' : 'Поверхность'}</small></p>
        `;
    }

    const adminSectorName = document.getElementById('admin-sector-name');
    const adminFactionSelect = document.getElementById('admin-faction-select');
    const adminDesc = document.getElementById('admin-sector-desc');
    const adminMarks = document.getElementById('admin-sector-marks');

    if (adminSectorName) adminSectorName.innerText = sector.name;
    if (adminFactionSelect) adminFactionSelect.value = sector.controllingFactionId;
    if (adminDesc) adminDesc.value = sector.description || "";
    if (adminMarks) adminMarks.value = sector.gmMarks || "";
}

async function saveSectorChanges() {
    if (!selectedSectorId) {
        alert("Сначала выберите сектор на карте!");
        return;
    }

    const newFactionId = parseInt(document.getElementById('admin-faction-select').value);
    const newDesc = document.getElementById('admin-sector-desc').value;
    const newMarks = document.getElementById('admin-sector-marks').value;

    const updateData = {
        controllingFactionId: newFactionId,
        description: newDesc,
        gmMarks: newMarks
    };

    try {
        const response = await fetch(`/api/map/${selectedSectorId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(updateData)
        });

        if (response.ok) {
            alert("Данные успешно сохранены в командный терминал!");
            loadMap();
        } else {
            alert("Не удалось сохранить изменения.");
        }
    } catch (error) {
        console.error("Ошибка админки:", error);
    }
}

document.addEventListener("DOMContentLoaded", () => {
    loadMap();
    checkAuth();
});