// ── Toast notification system ─────────────────────────────────
function showToast(message, type = "success") {
    const container = document.getElementById("toast-container");
    if (!container) return;
    
    const id = "toast-" + Date.now();
    const icons = {
        success: "bi-check-circle-fill",
        danger: "bi-exclamation-triangle-fill",
        warning: "bi-exclamation-circle-fill",
        info: "bi-info-circle-fill"
    };
    
    const html = `
        <div id="${id}" class="toast align-items-center text-bg-${type} border-0 show" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="bi ${icons[type] ?? icons.info} me-2"></i>${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;
    
    container.insertAdjacentHTML("beforeend", html);
    
    setTimeout(() => {
        const el = document.getElementById(id);
        if (el) el.remove();
    }, 5000);
}

// ── Auto-show TempData toasts injected by server ──────────────
document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("[data-toast]").forEach(el => {
        showToast(el.dataset.toast, el.dataset.toastType ?? "success");
    });
});

// ── Confirm dialog helper for delete buttons ──────────────────
document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll("[data-confirm]").forEach(btn => {
        btn.addEventListener("click", e => {
            if (!confirm(btn.dataset.confirm)) {
                e.preventDefault();
            }
        });
    });
});