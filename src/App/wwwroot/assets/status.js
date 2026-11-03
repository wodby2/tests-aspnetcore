// Fills the runtime details from the JSON API.
fetch("/api/status")
  .then((response) => response.json())
  .then((status) => {
    document.getElementById("framework").textContent = status.framework;
    document.getElementById("environment").textContent = status.environment;
  })
  .catch(() => {});
