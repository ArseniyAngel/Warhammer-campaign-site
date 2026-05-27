document.addEventListener("DOMContentLoaded", async () => {
    try {
        await loadGlobalTraits();
        await loadMyRoster();
    } catch (err) {
        console.error("Ошибка при инициализации штаба:", err);
    }
});

let mySquads = [];
let allUsersList = [];
let dbTraits = [];
let isUserAdminFromDB = false;

async function loadGlobalTraits() {
    try {
        const response = await fetch('/api/squads/traits-list');
        if (response.ok) dbTraits = await response.json();
    } catch (e) {
        console.error("Ошибка загрузки трейтов:", e);
    }
}

async function buyUpgrade(squadId) {
    const selectEl = document.getElementById(`shop-${squadId}`);
    if (!selectEl) return;
    const traitId = parseInt(selectEl.value);
    if (!traitId || traitId === 0) return alert("Выберите улучшение для покупки!");

    const response = await fetch('/api/squads/buy-upgrade', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ squadId, traitId })
    });

    if (response.ok) {
        alert("Модернизация успешно приобретена! Ресурсы списаны.");
        await loadMyRoster();
    } else {
        const errorText = await response.text();
        alert("Ошибка покупки: " + errorText);
    }
}

async function loadMyRoster() {
    try {
        const response = await fetch('/api/squads/my-squads');
        if (response.status === 401) return;

        const data = await response.json();
        mySquads = data.squads || [];

        // Проверка флага администратора из базы данных
        if (data.isGMSession === true) {
            isUserAdminFromDB = true;
            const zone = document.getElementById('gm-roster-panel');
            if (zone) {
                zone.style.display = "block";
                if (allUsersList.length === 0) {
                    await loadAdminUsersList();
                }
            }
        }

        // Синхронизация названия фракции
        const factionLabel = document.getElementById('player-faction-name');
        if (factionLabel) {
            factionLabel.innerText = data.factionName || "Новобранец";

            const picker = document.getElementById('faction-picker-zone');
            if (picker) {
                if (data.factionName && (data.factionName.includes("Неизвестная") || data.factionName.includes("Новобранец") || data.factionName.includes("новобранец"))) {
                    picker.style.display = "block";
                } else {
                    picker.style.display = "none";
                }
            }
        }

        // Отображение баланса игрока
        const balanceLabel = document.getElementById('player-points');
        if (balanceLabel) {
            balanceLabel.innerText = data.pointsBalance !== undefined ? data.pointsBalance : 0;
        }

        // Отображение названия валюты ресурсов фракции
        const resourceNameLabel = document.getElementById('player-points-name');
        if (resourceNameLabel) {
            resourceNameLabel.innerText = data.pointsName || "Ресурсы Снабжения";
        }

        // Подсчет полной стоимости текущего ростера
        const totalPtsLabel = document.getElementById('total-army-pts');
        if (totalPtsLabel) {
            const totalPoints = mySquads.reduce((sum, s) => sum + (s.pointsCost || 0), 0);
            totalPtsLabel.innerText = totalPoints;
        }

        // Отрисовка списка отрядов на фронтенде
        const squadListContainer = document.getElementById('my-squads-list');
        if (squadListContainer) {
            if (mySquads.length === 0) {
                squadListContainer.innerHTML = "<p style='color: #666; font-style: italic; padding: 20px;'>Ваш ростер пуст. Добавьте первый отряд слева!</p>";
                return;
            }

            squadListContainer.innerHTML = mySquads.map(s => {
                const uType = s.unitType || "Infantry";
                const cName = s.customName || "Без имени";
                const typeInfo = s.type || "";
                const pCost = s.pointsCost || 0;
                const bCost = s.basePointsCost || 0;

                let upgradesHtml = s.upgrades && s.upgrades.length > 0
                    ? s.upgrades.map(u => {
                        // 1. Берем сырое описание из БД
                        let cleanDescription = u.description || "Нет описания";

                        // 2. Очищаем Markdown-звездочки (**текст** или *текст* -> текст) для корректного отображения в title
                        cleanDescription = cleanDescription.replace(/\*\*|\*/g, "");

                        // 3. Экранируем кавычки, чтобы они не ломали HTML-атрибут
                        cleanDescription = cleanDescription.replace(/"/g, '&quot;').replace(/'/g, '&#39;');

                        // Возвращаем бейдж со стандартным атрибутом title
                        return `
            <span class="badge upgrade" 
                  style="background: rgba(77,184,255,0.1); color: #4db8ff; border: 1px solid rgba(77,184,255,0.3); padding: 3px 8px; border-radius: 4px; font-size: 0.8rem; margin-right: 5px; display: inline-block; margin-bottom: 5px; cursor: help;" 
                  title="${cleanDescription}">
                ${u.name} (+${u.ptsModifier} pts)
            </span>
        `;
                    }).join('')
                    : "<span style='color:#555; font-style:italic; font-size:0.85rem;'>Модернизаций нет</span>";
                let scarsHtml = s.scars && s.scars.length > 0
                    ? s.scars.map(sc => `<span class="badge scar" style="background: rgba(229,57,53,0.1); color: #e53935; border: 1px solid rgba(229,57,53,0.3); padding: 3px 8px; border-radius: 4px; font-size: 0.8rem; margin-right: 5px; display: inline-block; margin-bottom: 5px;" title="${sc.description || ''}">Шрам: ${sc.id}</span>`).join('')
                    : "";

                const availableUpgradesForShop = dbTraits.filter(t => {
                    if (t.type !== "Upgrade") return false;
                    const factionMatch = (t.factionName === data.factionName || t.factionName === "All");
                    const typeMatch = (t.unitTypeRestriction === "All" || t.unitTypeRestriction.includes(typeInfo));
                    const alreadyHas = (s.upgrades || []).some(up => up.id === t.id);

                    return factionMatch && typeMatch && !alreadyHas;
                });

                let shopDropdownHtml = "";
                if (availableUpgradesForShop.length > 0) {
                    // Найди этот кусок в loadMyRoster и замени стиль контейнера на flex-wrap: wrap
                    shopDropdownHtml = `
    <div style="margin-top: 15px; padding-top: 12px; border-top: 1px dashed #2c3542; display: flex; flex-wrap: wrap; gap: 10px; align-items: center;">
        <select id="shop-${s.id}" style="flex: 1; min-width: 200px; padding: 8px; background: #0c0f13; color: #fff; border: 1px solid #3a4454; font-size: 0.85rem; border-radius: 4px;">
            <option value="0">-- Купить модернизацию фракции --</option>
            ${availableUpgradesForShop.map(t => `<option value="${t.id}">[Цена: ${t.fractionPointsCost} ${data.pointsName || 'ОФ'}] ${t.name} (+${t.ptsModifier} pts)</option>`).join('')}
        </select>
        <button onclick="buyUpgrade(${s.id})" style="padding: 8px 15px; background: #4db8ff; color: #000; font-weight: bold; border: none; border-radius: 4px; cursor: pointer; font-size: 0.85rem; min-width: 80px;">Купить</button>
    </div>
`;
                } else {
                    shopDropdownHtml = `<p style="color: #555; font-size: 0.8rem; font-style: italic; margin-top: 10px;">Доступных уникальных улучшений фракции нет.</p>`;
                }

                return `
                    <div class="card squad-card" style="margin-bottom: 15px; background: #13171e; border: 1px solid #232d38; padding: 15px; border-radius: 6px; box-sizing: border-box;">
                        <div style="display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid #232d38; padding-bottom: 8px; margin-bottom: 10px;">
                            <div>
                                <h3 style="margin:0; color:#ff9800; font-size: 1.15rem;">${cName}</h3>
                                <small style="color:#888;">${uType} [${typeInfo}]</small>
                            </div>
                            <div style="text-align: right;">
                                <span class="pts-tag" style="background: #ff9800; color: #000; padding: 3px 8px; font-weight: bold; border-radius: 3px; font-size: 0.9rem;">${pCost} pts</span><br>
                                <small style="color:#555; font-size:0.75rem;">база: ${bCost} pts</small>
                            </div>
                        </div>
                        
                        <div style="margin-top: 10px; display: flex; flex-wrap: wrap; gap: 6px;">
                            ${upgradesHtml}
                            ${scarsHtml}
                        </div>

                        ${shopDropdownHtml}
                    </div>
                `;
            }).join('');
        }
    } catch (e) {
        console.error("Критическая ошибка рендеринга ростера:", e);
    }
}

async function addSquad() {
    const customNameEl = document.getElementById('squad-custom-name');
    const unitTypeEl = document.getElementById('squad-unit-type');
    const typeEl = document.getElementById('squad-type');
    const pointsCostEl = document.getElementById('squad-pts');

    if (!customNameEl || !unitTypeEl || !typeEl || !pointsCostEl) return;

    const customName = customNameEl.value.trim();
    const unitType = unitTypeEl.value.trim();
    const type = typeEl.value;
    const pointsCost = parseInt(pointsCostEl.value);

    if (!customName || !unitType || !pointsCost) return alert("Заполните все поля патентного запроса!");

    const response = await fetch('/api/squads/add', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ customName, unitType, type, pointsCost })
    });

    if (response.ok) {
        customNameEl.value = "";
        unitTypeEl.value = "";
        pointsCostEl.value = "100";
        await loadMyRoster();
    } else {
        alert("Не удалось добавить отряд в базу данных.");
    }
}

async function saveMyFaction() {
    const selectEl = document.getElementById('register-faction-select');
    if (!selectEl) return;

    const factionId = parseInt(selectEl.value);
    if (!factionId) return alert("Выберите легион/фракцию!");

    const response = await fetch('/api/squads/set-faction', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: factionId
    });

    if (response.ok) {
        alert("Фракция успешно утверждена за вашим терминалом!");
        window.location.reload();
    } else {
        alert("Ошибка регистрации фракции.");
    }
}

function checkGMRole() {
    // Метод оставлен пустым, так как роль проверяется на стороне бэкенда в loadMyRoster()
}

async function loadAdminUsersList() {
    try {
        const response = await fetch('/api/squads/admin/users');
        if (response.ok) {
            allUsersList = await response.json();
            const select = document.getElementById('gm-user-select');
            if (select) {
                select.innerHTML = '<option value="0">-- Выберите Военачальника --</option>' +
                    allUsersList.map(u => `<option value="${u.id}">${u.username} (${u.role})</option>`).join('');
            }
        }
    } catch (err) {
        console.error("Ошибка загрузки пользователей для ГМ:", err);
    }
}

async function removeTraitByGM(squadId, traitId, traitName) {
    if (!confirm(`Вы действительно хотите удалить "${traitName}" у этого отряда?`)) return;

    try {
        const response = await fetch(`/api/squads/admin/remove-trait/${squadId}/${traitId}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            alert("Компонент успешно удален!");
            await loadSelectedUserForAdmin();
            await loadMyRoster();
        } else {
            const errText = await response.text();
            alert(errText || "Не удалось удалить компонент.");
        }
    } catch (e) { console.error(e); }
}

async function loadSelectedUserForAdmin() {
    const selectEl = document.getElementById('gm-user-select');
    if (!selectEl) return;

    const userId = parseInt(selectEl.value);
    if (!userId || userId === 0) {
        const inspector = document.getElementById('gm-squads-inspector');
        if (inspector) inspector.innerHTML = "<p style='color: #666; font-style: italic;'>Укажите военачальника в списке для сканирования структуры армии.</p>";
        return;
    }

    const user = allUsersList.find(u => u.id === userId);
    if (!user) return;

    document.getElementById('gm-target-user-pts').innerText = user.factionPointsBalance !== undefined ? user.factionPointsBalance : 0;

    const response = await fetch(`/api/squads/admin/user-squads/${userId}`);
    const squads = await response.json();

    const inspector = document.getElementById('gm-squads-inspector');
    if (!inspector) return;

    if (squads.length === 0) {
        inspector.innerHTML = "<p style='color: #666; padding: 15px;'>У этого военачальника нет активных отрядов.</p>";
        return;
    }

    const scarsList = dbTraits.filter(t => t.type === "Scar");

    inspector.innerHTML = squads.map(s => {
        let upgradesHtml = s.upgrades && s.upgrades.length > 0
            ? s.upgrades.map(u => `
                <span style="display:inline-flex; align-items:center; background: rgba(77,184,255,0.08); color: #4db8ff; padding: 4px 8px; border-radius: 4px; font-size: 0.85rem; border: 1px solid rgba(77,184,255,0.2); font-weight:bold; margin: 2px;">
                    ${u.name} (+${u.ptsModifier} pts)
                    <button onclick="removeTraitByGM(${s.id}, ${u.id}, '${u.name}')" style="background:transparent; border:none; color:#e53935; margin-left:6px; cursor:pointer; font-weight:bold; font-size:0.8rem; padding:0 2px;">❌</button>
                </span>
            `).join('')
            : "<span style='color: #555; font-style: italic;'>Модернизаций нет</span>";

        let scarsHtml = s.scars && s.scars.length > 0
            ? s.scars.map(sc => `
                <div style="color: #e53935; font-size: 0.85rem; margin-top:4px; padding-left:8px; border-left: 2px solid #e53935; display:flex; justify-content:space-between; align-items:center;">
                    <span>•<strong>${sc.id}</strong>. ${sc.description}</span>
                    <button onclick="removeTraitByGM(${s.id}, ${sc.id}, '${sc.name}')" style="background:transparent; border:none; color:#e53935; cursor:pointer; font-weight:bold; font-size:0.85rem; padding:0 5px;" title="Исцелить шрам">❌</button>
                </div>
            `).join('')
            : "<span style='color: #666; font-style: italic;'>Нет наложенных шрамов</span>";

        return `
            <div class="gm-squad-card-expanded" style="margin-bottom: 25px; background: #0b0d10; border: 1px solid #444; padding: 20px; border-radius: 6px; display: block; clear: both; box-sizing: border-box;">
                <div style="display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid #222; padding-bottom: 10px; margin-bottom: 15px;">
                    <span style="color: #ff9800; font-size: 1.3rem; font-weight: bold;">${s.customName}</span>
                    <span style="color: #aaa; font-size: 0.9rem; background: #111; padding: 4px 10px; border-radius:4px; border: 1px solid #222;">${s.unitType} (${s.type})</span>
                </div>
                <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin-bottom: 20px; background: #111317; padding: 15px; border-radius: 4px; border: 1px solid #1c2129;">
                    <div>
                        <strong style="color: #4db8ff; display: block; margin-bottom: 6px; font-size: 0.85rem; text-transform: uppercase;">🛡️ Текущие Улучшения:</strong>
                        <div style="display: flex; flex-wrap: wrap; gap: 4px;">${upgradesHtml}</div>
                    </div>
                    <div>
                        <strong style="color: #e53935; display: block; margin-bottom: 6px; font-size: 0.85rem; text-transform: uppercase;">🤕 Список шрамов:</strong>
                        <div style="display: flex; flex-direction: column; gap: 4px;">${scarsHtml}</div>
                    </div>
                </div>
                <div style="display: grid; grid-template-columns: 2.5fr 1fr 1.2fr; gap: 15px; align-items: end; background: rgba(255,255,255,0.01); padding: 12px; border-radius: 4px;">
                    <div style="display: flex; flex-direction: column; gap: 5px;">
                        <label style="font-size: 0.85rem; color: #aaa; font-weight: bold;">Наложить новый Боевой Шрам:</label>
                        <select id="gm-scars-${s.id}" style="width: 100%; padding: 10px; background: #000; color: #fff; border: 1px solid #444; font-size: 0.9rem; border-radius:4px;">
                            <option value="0">-- Выбрать ID шрама из базы --</option>
                            ${scarsList.map(t => `<option value="${t.id}">${t.id} — ${t.description}</option>`).join('')}
                        </select>
                    </div>
                    <div style="display: flex; flex-direction: column; gap: 5px;">
                        <label style="font-size: 0.85rem; color: #aaa; font-weight: bold;">База (pts):</label>
                        <input type="number" id="gm-pts-${s.id}" value="${s.pointsCost}" style="width: 100%; padding: 9px; background: #000; color: #fff; border: 1px solid #444; text-align: center; font-size: 0.9rem; font-weight: bold; border-radius:4px;">
                    </div>
                    <div>
                        <button onclick="saveSquadByGM(${s.id})" style="width: 100%; padding: 10px 0; background: #e53935; color: #fff; border: none; font-weight: bold; cursor: pointer; border-radius: 4px; font-size: 0.95rem; text-transform: uppercase;">Применить</button>
                    </div>
                </div>
            </div>
        `;
    }).join('');
}

async function changePointsGM(delta) {
    const userId = parseInt(document.getElementById('gm-user-select').value);
    if (!userId) return alert("Сначала выберите игрока!");

    const response = await fetch(`/api/squads/admin/add-user-points/${userId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: delta
    });

    if (response.ok) {
        const data = await response.json();
        document.getElementById('gm-target-user-pts').innerText = data.newBalance;
        const user = allUsersList.find(u => u.id === userId);
        if (user) user.factionPointsBalance = data.newBalance;
        await loadMyRoster();
    }
}

async function saveSquadByGM(squadId) {
    const pointsCost = parseInt(document.getElementById(`gm-pts-${squadId}`).value);
    const scarId = parseInt(document.getElementById(`gm-scars-${squadId}`).value);

    const modData = { pointsCost, scarId };

    const response = await fetch(`/api/squads/admin/mod-squad/${squadId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(modData)
    });

    if (response.ok) {
        alert("Изменения успешно применены Гейм-Мастером!");
        await loadSelectedUserForAdmin();
        await loadMyRoster();
    }
}
// Переключатель отображения ввода фракции
function toggleFactionInput(isGeneralStr) {
    const block = document.getElementById('new-trait-faction-block');
    if (!block) return;

    if (isGeneralStr === "false") {
        block.style.display = "flex"; // Показываем поле ввода фракции
    } else {
        block.style.display = "none";  // Прячем для общей прокачки
    }
}

// Функция создания новой прокачки
async function createNewTraitByGM() {
    const nameEl = document.getElementById('new-trait-name');
    const descEl = document.getElementById('new-trait-desc');
    const restrictionEl = document.getElementById('new-trait-restriction');
    const pointsCostEl = document.getElementById('new-trait-points-cost');
    const ptsModifierEl = document.getElementById('new-trait-pts');
    const isGeneralEl = document.getElementById('new-trait-general-select');
    const factionNameEl = document.getElementById('new-trait-faction-name');

    if (!nameEl || !descEl || !restrictionEl || !pointsCostEl || !ptsModifierEl || !isGeneralEl || !factionNameEl) return;

    const name = nameEl.value.trim();
    const description = descEl.value.trim();
    const unitTypeRestriction = restrictionEl.value.trim();
    const fractionPointsCost = parseInt(pointsCostEl.value);
    const ptsModifier = parseInt(ptsModifierEl.value);
    const isGeneral = isGeneralEl.value === "true";
    const factionName = factionNameEl.value.trim();

    if (!name || !description || !unitTypeRestriction) {
        return alert("Заполните Название, Описание и Ограничение типа!");
    }

    if (!isGeneral && !factionName) {
        return alert("Для уникальной модернизации необходимо ввести точное имя фракции!");
    }

    // Собираем объект для отправки в C# контроллер
    const traitDto = {
        name,
        description,
        unitTypeRestriction,
        ptsModifier,
        fractionPointsCost,
        isGeneral,
        factionName: isGeneral ? "All" : factionName
    };

    try {
        const response = await fetch('/api/squads/admin/add-trait', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(traitDto)
        });

        if (response.ok) {
            alert(`Директива исполнена! Модернизация "${name}" добавлена в Кодекс кампании.`);

            // Сбрасываем форму для удобства следующего ввода
            nameEl.value = "";
            descEl.value = "";
            restrictionEl.value = "All";
            factionNameEl.value = "";

            // Сразу же обновляем глобальный список трейтов в памяти фронтенда, чтобы игроки видели изменения
            await loadGlobalTraits();
            await loadMyRoster();
        } else {
            const errText = await response.text();
            alert("Ошибка создания: " + errText);
        }
    } catch (e) {
        console.error("Ошибка при связи с сервером кузницы:", e);
        alert("Не удалось связаться с сервером.");
    }
}