const rows = document.querySelector("#token-rows");
const emptyState = document.querySelector("#empty-state");
const tableWrap = document.querySelector("#table-wrap");
const count = document.querySelector("#token-count");
const form = document.querySelector("#create-form");
const input = document.querySelector("#token-name");
const createButton = document.querySelector("#create-button");
const tokenBox = document.querySelector("#token-box");
const tokenValue = document.querySelector("#token-value");
const copyButton = document.querySelector("#copy-button");
const toast = document.querySelector("#toast");
let latestToken = "";
let toastTimer;
const visibleTokens = new Map(
  JSON.parse(sessionStorage.getItem("visibleTokens") || "[]")
);

function apiUrl(path) {
  return new URL(path, `${window.location.origin}${window.location.pathname}`).toString();
}

async function request(path, options = {}) {
  const response = await fetch(apiUrl(path), {
    headers: {
      "Content-Type": "application/json",
      ...(options.headers || {})
    },
    ...options
  });

  if (!response.ok) {
    let message = `${response.status} ${response.statusText}`;
    try {
      const body = await response.json();
      message = body.error || message;
    } catch {
    }
    throw new Error(message);
  }

  return response.status === 204 ? null : response.json();
}

function formatDate(value) {
  if (!value) {
    return "Never";
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short"
  }).format(new Date(value));
}

function showToast(message) {
  toast.textContent = message;
  toast.classList.add("visible");
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => toast.classList.remove("visible"), 2600);
}

function rememberToken(id, rawToken) {
  visibleTokens.set(id, rawToken);
  sessionStorage.setItem("visibleTokens", JSON.stringify([...visibleTokens]));
}

async function copyRawToken(rawToken) {
  await navigator.clipboard.writeText(rawToken);
  showToast("Token copied");
}

async function loadTokens() {
  const tokens = await request("api/admin/tokens");
  const activeCount = tokens.filter(token => !token.revokedAt).length;
  count.textContent = `${activeCount} active`;

  rows.replaceChildren(...tokens.map(renderToken));
  const hasTokens = tokens.length > 0;
  emptyState.hidden = hasTokens;
  tableWrap.hidden = !hasTokens;
}

function renderToken(token) {
  const tr = document.createElement("tr");
  const revoked = Boolean(token.revokedAt);

  tr.innerHTML = `
    <td data-label="Name"><div class="name"></div></td>
    <td class="muted" data-label="Created"></td>
    <td class="muted" data-label="Last used"></td>
    <td data-label="Status"><span class="status ${revoked ? "revoked" : "active"}">${revoked ? "Revoked" : "Active"}</span></td>
    <td data-label="Actions"><div class="actions"></div></td>
  `;

  tr.querySelector(".name").textContent = token.name;
  tr.children[1].textContent = formatDate(token.createdAt);
  tr.children[2].textContent = revoked ? formatDate(token.revokedAt) : formatDate(token.lastUsedAt);

  const actions = tr.querySelector(".actions");
  if (revoked) {
    const button = document.createElement("button");
    button.className = "button danger";
    button.type = "button";
    button.textContent = "Delete";
    button.addEventListener("click", () => deleteRevokedToken(token.id));
    actions.append(button);
  } else {
    const rawToken = visibleTokens.get(token.id);
    if (rawToken) {
      const copy = document.createElement("button");
      copy.className = "button secondary";
      copy.type = "button";
      copy.textContent = "Copy";
      copy.addEventListener("click", () => copyRawToken(rawToken));
      actions.append(copy);
    } else {
      const shownOnce = document.createElement("span");
      shownOnce.className = "muted";
      shownOnce.textContent = "Shown once";
      actions.append(shownOnce);
    }

    const button = document.createElement("button");
    button.className = "button danger";
    button.type = "button";
    button.textContent = "Revoke";
    button.addEventListener("click", () => revokeToken(token.id));
    actions.append(button);
  }

  return tr;
}

async function revokeToken(id) {
  await request(`api/admin/tokens/${id}`, { method: "DELETE" });
  visibleTokens.delete(id);
  sessionStorage.setItem("visibleTokens", JSON.stringify([...visibleTokens]));
  await loadTokens();
  showToast("Token revoked");
}

async function deleteRevokedToken(id) {
  await request(`api/admin/tokens/${id}/record`, { method: "DELETE" });
  visibleTokens.delete(id);
  sessionStorage.setItem("visibleTokens", JSON.stringify([...visibleTokens]));
  await loadTokens();
  showToast("Token deleted");
}

form.addEventListener("submit", async event => {
  event.preventDefault();
  createButton.disabled = true;
  try {
    const created = await request("api/admin/tokens", {
      method: "POST",
      body: JSON.stringify({ name: input.value })
    });
    latestToken = created.token;
    rememberToken(created.id, created.token);
    tokenValue.textContent = latestToken;
    tokenBox.classList.add("visible");
    input.value = "";
    await loadTokens();
    showToast("Token created");
  } catch (error) {
    showToast(error.message);
  } finally {
    createButton.disabled = false;
    input.focus();
  }
});

copyButton.addEventListener("click", async () => {
  if (!latestToken) {
    return;
  }

  await copyRawToken(latestToken);
});

loadTokens().catch(error => showToast(error.message));
