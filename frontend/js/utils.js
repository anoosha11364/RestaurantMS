/* =====================================================================
   RestaurantMS — Shared Utilities
   ===================================================================== */

const BASE_URL = 'http://localhost:5000/api'

/* ── API Helper ──────────────────────────────────────────────────── */
const api = {
  async get(endpoint) {
    const res = await fetch(`${BASE_URL}${endpoint}`)
    if (!res.ok) throw await res.json()
    return res.json()
  },
  async post(endpoint, body) {
    const res = await fetch(`${BASE_URL}${endpoint}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body)
    })
    if (!res.ok) throw await res.json()
    return res.json()
  },
  async put(endpoint, body) {
    const res = await fetch(`${BASE_URL}${endpoint}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body)
    })
    if (!res.ok) throw await res.json()
    return res.json()
  },
  async patch(endpoint, body) {
    const res = await fetch(`${BASE_URL}${endpoint}`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body)
    })
    if (!res.ok) throw await res.json()
    return res.json()
  },
  async delete(endpoint) {
    const res = await fetch(`${BASE_URL}${endpoint}`, { method: 'DELETE' })
    if (!res.ok) throw await res.json()
    return res.ok
  }
}

/* ── Toast ───────────────────────────────────────────────────────── */
function toast(message, type = 'success') {
  const icons = { success: '✅', error: '❌', warning: '⚠️', info: 'ℹ️' }
  const container = document.getElementById('toast-container')
  const el = document.createElement('div')
  el.className = `toast ${type !== 'success' ? type : ''}`
  el.innerHTML = `<span>${icons[type]}</span><span>${message}</span>`
  container.appendChild(el)
  setTimeout(() => el.remove(), 3100)
}

/* ── Modal ───────────────────────────────────────────────────────── */
function openModal(id)  {
  const m = document.getElementById(id)
  m.classList.add('open')
  document.body.style.overflow = 'hidden'
}
function closeModal(id) {
  const m = document.getElementById(id)
  m.classList.remove('open')
  document.body.style.overflow = ''
}

// close when clicking backdrop
document.addEventListener('click', e => {
  if (e.target.classList.contains('modal-overlay')) {
    e.target.classList.remove('open')
    document.body.style.overflow = ''
  }
})

/* ── Auth Helpers ────────────────────────────────────────────────── */
function getUser() {
  try { return JSON.parse(localStorage.getItem('rms_user') || 'null') }
  catch { return null }
}

function setUser(data) { localStorage.setItem('rms_user', JSON.stringify(data)) }

function logout() {
  localStorage.clear()
  window.location.href = '../index.html'
}

function requireAuth(role = null) {
  const user = getUser()
  if (!user) { window.location.href = '../index.html'; return null }
  if (role && user.role !== role) { window.location.href = '../index.html'; return null }
  return user
}

/* ── Status Badge ────────────────────────────────────────────────── */
function statusBadge(status) {
  return `<span class="badge badge-${status.toLowerCase()}">${status}</span>`
}

/* ── Render Navbar User Info ─────────────────────────────────────── */
function initNavbar() {
  const user = getUser()
  const nameEl = document.getElementById('nav-user-name')
  const avatarEl = document.getElementById('nav-avatar')
  if (user && nameEl) nameEl.textContent = user.fullName || 'User'
  if (user && avatarEl) avatarEl.textContent = (user.fullName || 'U')[0].toUpperCase()

  // highlight active nav link
  const links = document.querySelectorAll('.navbar-links a')
  links.forEach(l => {
    if (l.href === window.location.href) l.classList.add('active')
  })
}

/* ── Empty State ─────────────────────────────────────────────────── */
function emptyState(icon, text) {
  return `<div class="empty-state"><div class="empty-state-icon">${icon}</div><p class="empty-state-text">${text}</p></div>`
}

/* ── Format Date ─────────────────────────────────────────────────── */
function fmtDate(d)     { return new Date(d).toLocaleDateString() }
function fmtDateTime(d) { return new Date(d).toLocaleString() }
function fmtCurrency(n) { return `Rs. ${parseFloat(n).toLocaleString('en-PK', { minimumFractionDigits: 2 })}` }

/* ── Confirm Dialog ──────────────────────────────────────────────── */
function confirmAction(message) { return window.confirm(message) }

/* ── Loader on button ────────────────────────────────────────────── */
function setLoading(btn, loading) {
  if (loading) {
    btn.dataset.originalText = btn.innerHTML
    btn.innerHTML = '<span class="loader"></span>'
    btn.disabled = true
  } else {
    btn.innerHTML = btn.dataset.originalText || 'Submit'
    btn.disabled = false
  }
}

/* ── On DOM ready ────────────────────────────────────────────────── */
document.addEventListener('DOMContentLoaded', () => {
  initNavbar()

  // logout buttons
  document.querySelectorAll('[data-logout]').forEach(btn => {
    btn.addEventListener('click', logout)
  })

  // modal close buttons
  document.querySelectorAll('[data-close-modal]').forEach(btn => {
    btn.addEventListener('click', () => closeModal(btn.dataset.closeModal))
  })
})
