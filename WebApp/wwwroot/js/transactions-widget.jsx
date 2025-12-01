const { useMemo, useState, useEffect } = React;

function TransactionsLoadMoreWidget() {
  const all = Array.isArray(window.transactionsData) ? window.transactionsData : [];
  const STEP = 20;

  // Антифрод токен (для POST Delete) — без optional chaining
  var tokenInput = document.querySelector('#anti-forgery-form input[name="__RequestVerificationToken"]');
  var csrfToken = tokenInput ? tokenInput.value : "";

  const [visibleCount, setVisibleCount] = useState(Math.min(STEP, all.length));
  const [showScrollTop, setShowScrollTop] = useState(false);

  // показуємо кнопку, якщо прокрутка > 400px
  useEffect(() => {
    function onScroll() {
      setShowScrollTop(window.scrollY > 400);
    }
    onScroll();
    window.addEventListener("scroll", onScroll, { passive: true });
    return () => window.removeEventListener("scroll", onScroll);
  }, []);

  const visible = useMemo(() => all.slice(0, visibleCount), [all, visibleCount]);
  const hasMore = visibleCount < all.length;

  const handleShowMore = () => setVisibleCount((c) => Math.min(c + STEP, all.length));

  const scrollToTop = () => {
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  if (!all.length) {
    return <div className="alert alert-info m-0">Не знайдено жодної транзакції.</div>;
  }

  return (
    <div style={{ position: "relative" }}>
      <div className="card border-primary">
        <div className="card-header bg-primary text-white d-flex justify-content-between align-items-center">
          <strong>📄 Транзакції</strong>
          <span className="text-white-50">Показано {visible.length} з {all.length}</span>
        </div>

        <div className="card-body p-0">
          <table className="table table-striped mb-0">
            <thead>
              <tr>
                <th>#</th>
                <th>Назва</th>
                <th>Сума</th>
                <th>Сума після податку</th>
                <th>Категорія</th>
                <th>Користувач</th>
                <th>Створено</th>
                <th></th>
              </tr>
            </thead>

            <tbody>
              {visible.map((t) => (
                <tr key={t.id}>
                  <td>{t.id}</td>
                  <td>{t.name}</td>
                  <td>{t.sum}</td>
                  <td>{t.sumAfterTax}</td>
                  <td>{t.category || "-"}</td>
                  <td>{t.user || "-"}</td>
                  <td>{t.createdAt}</td>
                  <td style={{ whiteSpace: "nowrap" }}>
                    <a className="btn btn-warning btn-sm me-2" href={`/Transaction/Update/${t.id}`}>
                      ✏️ Редагувати
                    </a>

                    <form
                      method="post"
                      action={`/Transaction/Delete/${t.id}`}
                      style={{ display: "inline" }}
                      onSubmit={(e) => {
                        if (!confirm("Видалити цю транзакцію?")) e.preventDefault();
                      }}
                    >
                      <input type="hidden" name="__RequestVerificationToken" value={csrfToken} />
                      <button type="submit" className="btn btn-danger btn-sm">
                        🗑️ Видалити
                      </button>
                    </form>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="card-footer d-flex justify-content-center">
          {hasMore ? (
            <button className="btn btn-outline-primary" onClick={handleShowMore}>
              Показати ще 20
            </button>
          ) : (
            <div className="text-muted">Ви досягли кінця списку — транзакції закінчилися ✅</div>
          )}
        </div>
      </div>

      {/* Кнопка "вгору" */}
      {showScrollTop ? (
        <button
          type="button"
          className="btn btn-primary"
          onClick={scrollToTop}
          aria-label="Повернутися вгору"
          title="Вгору"
          style={{
            position: "fixed",
            right: "84px",
            bottom: "36px",
            zIndex: 9999,
            borderRadius: "999px",
            width: "44px",
            height: "44px",
            padding: "0",
            boxShadow: "0 5px 20px rgba(0,0,0,0.2)"
          }}
        >
          𖤂
        </button>
      ) : null}
    </div>
  );
}

function mountTransactionsWidget() {
  const rootElement = document.getElementById("transactions-react-widget");
  if (!rootElement || !window.React || !window.ReactDOM) return;

  const root = ReactDOM.createRoot(rootElement);
  root.render(<TransactionsLoadMoreWidget />);
}

if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", mountTransactionsWidget);
} else {
  mountTransactionsWidget();
}
