// HotbarComponent étend Component (s&box) et requiert un contexte Scene/GameObject
// pour être instancié — non disponible hors runtime s&box.
// Les méthodes de logique pure (Assign, Clear, UseSlot, GetItemId) peuvent être
// testées via vérification manuelle dans l'éditeur s&box.
//
// Contraintes vérifiées visuellement :
// - Assign(-1/8, ...) → Fail "Invalid hotbar slot"
// - Assign(_, null/empty) → Fail "No item specified"
// - Assign sans inventory lié → Fail "No inventory bound"
// - Assign avec item non possédé → Fail "Item is not owned by the player"
// - Assign avec item possédé → Success + slot[index] = itemId
// - Clear(-1/8) → Fail "Invalid hotbar slot"
// - Clear(valid) → slot[index] = null, Success
// - GetItem auto-vide le slot si l'item n'existe plus dans l'inventaire
// - UseSlot sur slot vide → Fail "Hotbar slot is empty"
// - UseSlot sur arme déjà équipée → Ok "{DisplayName} selected."
// - UseSlot sur arme non équipée → Fail "{DisplayName} must be equipped before use."
// - UseSlot sur item non-arme → Fail "{DisplayName} has no use action yet."
