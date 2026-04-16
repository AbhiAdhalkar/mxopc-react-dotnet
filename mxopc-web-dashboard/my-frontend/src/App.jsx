import { useEffect, useState } from "react";
import "./App.css";
import buttonItems from "./buttonTags.json";

function App() {
  const [tags, setTags] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const apiBaseUrl = import.meta.env.VITE_API_BASE_URL?.trim();

  useEffect(() => {
    let isMounted = true;

    if (!apiBaseUrl) {
      setError("VITE_API_BASE_URL is missing");
      setLoading(false);
      return;
    }

    const fetchTags = async () => {
      try {
        const response = await fetch(`${apiBaseUrl}/api/tags`, {
          headers: {
            "ngrok-skip-browser-warning": "true"
          }
        });

        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }

        const data = await response.json();

        if (isMounted) {
          setTags(Array.isArray(data) ? data : []);
          setError("");
          setLoading(false);
        }
      } catch (err) {
        if (isMounted) {
          setError(err.message || "Failed to fetch tags");
          setLoading(false);
        }
      }
    };

    fetchTags();
    const interval = setInterval(fetchTags, 500);

    return () => {
      isMounted = false;
      clearInterval(interval);
    };
  }, [apiBaseUrl]);

  const isOn = (value) => {
    if (value === true || value === 1) return true;
    if (value === false || value === 0 || value == null) return false;

    const normalized = String(value).trim().toLowerCase();
    return normalized === "1" || normalized === "true" || normalized === "on";
  };

  const getTag = (name) => tags.find((t) => t.tagName === name);
  const getTagValue = (name) => getTag(name)?.value;

  const toggleTag = async (tagName) => {
    try {
      if (!apiBaseUrl) {
        throw new Error("VITE_API_BASE_URL is missing");
      }

      const response = await fetch(`${apiBaseUrl}/api/tags/write`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "ngrok-skip-browser-warning": "true"
        },
        body: JSON.stringify({ tagName })
      });

      if (!response.ok) {
        const text = await response.text();
        throw new Error(text || `HTTP error! Status: ${response.status}`);
      }

      const result = await response.json();

      setTags((prev) =>
        prev.map((tag) =>
          tag.tagName === tagName
            ? {
                ...tag,
                value: result.value,
                quality: result.quality ?? "Good",
                timestamp: result.timestamp ?? new Date().toLocaleString(),
                error: null
              }
            : tag
        )
      );
    } catch (err) {
      alert(`Write failed: ${err.message}`);
    }
  };

  if (loading) return <h2 className="state">Loading...</h2>;
  if (error) return <h2 className="state error-text">Error: {error}</h2>;

  return (
    <div className="page">
      <header className="header">
        <h1>OPC Live Status</h1>
      </header>

      <div className="main-layout">
        <div className="left-column">
          <div className="grid">
            {tags.map((tag) => (
              <div className="card" key={tag.tagName}>
                <div className="card-top">
                  <span className="label">{tag.tagName}</span>
                  <span className={`pill ${isOn(tag.value) ? "on" : "off"}`}>
                    {isOn(tag.value) ? "ON" : "OFF"}
                  </span>
                </div>

                <div className="value-row">
                  <span className="value">{tag.value ?? "N/A"}</span>
                  <span className="meta">{tag.quality ?? "Unknown"}</span>
                </div>

                <div className="timestamp">{tag.timestamp ?? "No timestamp"}</div>
                {tag.error && <div className="error">{tag.error}</div>}
              </div>
            ))}
          </div>
        </div>

        <div className="right-column">
          <div className="button-panel">
            {buttonItems.map((item) => {
              const value = getTagValue(item.tagName);
              const on = isOn(value);

              return (
                <button
                  key={item.tagName}
                  className={`status-btn ${on ? "green" : "red"}`}
                  onClick={() => toggleTag(item.tagName)}
                >
                  {item.label}
                </button>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
}

export default App;