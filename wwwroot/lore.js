document.addEventListener("DOMContentLoaded", () => {
    checkGMRole();
    loadNews();
});

// Проверяем, является ли пользователь ГМ-ом, чтобы показать форму
async function checkGMRole() {
    try {
        const response = await fetch('/api/auth/me');
        const data = await response.json();

        if (data.role === "Admin") {
            document.getElementById('gm-news-panel').style.display = "block";
        }
    } catch (e) {
        console.error(e);
    }
}

// Загрузка новостей с сервера
async function loadNews() {
    const feed = document.getElementById('news-feed');
    try {
        const response = await fetch('/api/news');
        const news = await response.json();

        if (news.length === 0) {
            feed.innerHTML = "<p>В хрониках пока затишье. Гейм-Мастер еще не добавил записей.</p>";
            return;
        }

        feed.innerHTML = news.map(post => `
            <div class="news-post" style="border-bottom: 1px dashed #444; padding: 15px 0;">
                <h3 style="color: #ffb74d; margin-bottom: 5px;">${post.title}</h3>
                <small style="color: #888;">${new Date(post.createdAt).toLocaleString('ru-RU')}</small>
                <p style="margin-top: 10px; line-height: 1.5; white-space: pre-wrap;">${post.content}</p>
            </div>
        `).join('');

    } catch (error) {
        feed.innerHTML = "<p style='color:red;'>Не удалось загрузить хроники.</p>";
    }
}

// Отправка новой записи на бэкенд
async function submitNews() {
    const title = document.getElementById('news-title').value;
    const content = document.getElementById('news-content').value;

    if (!title || !content) return alert("Заполните заголовок и текст сводки!");

    try {
        const response = await fetch('/api/news', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ title, content })
        });

        if (response.ok) {
            alert("Новость добавлена!");
            document.getElementById('news-title').value = "";
            document.getElementById('news-content').value = "";
            loadNews(); // Обновляем список
        } else {
            alert("Ошибка. Возможно, у вас нет прав Гейм-Мастера.");
        }
    } catch (error) {
        console.error(error);
    }
}