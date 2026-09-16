import { useState, useEffect } from 'react';
import aspireLogo from '/Aspire.png';
import './App.css';

interface Restaurant {
    id: number;
    name: string;
    cuisine: string;
    rating: number;
}

function App() {
    const [restaurants, setRestaurants] = useState<Restaurant[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const fetchRestaurants = async () => {
        setLoading(true);
        setError(null);

        try {
            const response = await fetch('/api/restaurants');

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data: Restaurant[] = await response.json();
            setRestaurants(data);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to fetch restaurants');
            console.error('Error fetching restaurants:', err);
        } finally {
            setLoading(false);
        }
    };


    const placeOrder = async (restaurantId: number) => {
        try {
            const response = await fetch('/api/orders', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ restaurantId, items: ['Sample Item 1', 'Sample Item 2'] }),
            });
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            const order = await response.json();
            alert(`Order placed! Order ID: ${order.id}`);
        } catch (err) {
            alert('Failed to place order: ' + (err instanceof Error ? err.message : 'unknown error'));
        }
    };




    useEffect(() => {
        fetchRestaurants();
    }, []);

    return (
    <div className="app-container">
      <header className="app-header">
        <a
          href="https://aspire.dev"
          target="_blank"
          rel="noopener noreferrer"
          aria-label="Visit Aspire website (opens in new tab)"
          className="logo-link"
        >
          <img src={aspireLogo} className="logo" alt="Aspire logo" />
        </a>
        <h1 className="app-title">FoodExpress</h1>
        <p className="app-subtitle">Restaurants near you</p>
      </header>

      <main className="main-content">
        <section className="weather-section" aria-labelledby="restaurants-heading">
          <div className="card">
            <div className="section-header">
              <h2 id="restaurants-heading" className="section-title">Restaurants</h2>
              <button
                className="refresh-button"
                onClick={fetchRestaurants}
                disabled={loading}
                type="button"
              >
                <span>{loading ? 'Loading...' : 'Refresh'}</span>
              </button>
            </div>

            {error && (
              <div className="error-message" role="alert" aria-live="polite">
                <span>{error}</span>
              </div>
            )}

            {loading && restaurants.length === 0 && (
              <div className="loading-skeleton" role="status" aria-live="polite">
                {[...Array(6)].map((_, i) => (
                  <div key={i} className="skeleton-row" aria-hidden="true" />
                ))}
              </div>
            )}

            {restaurants.length > 0 && (
              <div className="weather-grid">
                {restaurants.map((r) => (
                  <article key={r.id} className="weather-card" aria-label={r.name}>
                    <h3 className="weather-date">{r.name}</h3>
                    <p className="weather-summary">{r.cuisine}</p>
                    <div className="weather-temps">
                      <div className="temp-group">
                        <span className="temp-value">★ {r.rating.toFixed(1)}</span>
                            </div>
                        </div>
                        <button
                            className="refresh-button"
                            onClick={() => placeOrder(r.id)}
                            type="button"
                        >
                            Order Now
                        </button>
                  </article>
                ))}
              </div>
            )}
          </div>
        </section>
      </main>

      <footer className="app-footer">
        <nav aria-label="Footer navigation">
          <a href="https://aspire.dev" target="_blank" rel="noopener noreferrer">
            Learn more about Aspire<span className="visually-hidden"> (opens in new tab)</span>
          </a>
        </nav>
      </footer>
    </div >
  );
}

export default App;