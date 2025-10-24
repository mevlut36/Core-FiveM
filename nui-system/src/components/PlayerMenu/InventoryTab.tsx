import { useState } from 'react'
import { Package, Play, Send } from 'lucide-react'
import { fetchNui } from '../../utils/fetchNui'

interface InventoryItem {
  name: string
  quantity: number
  type: string
  icon: string
}

interface InventoryTabProps {
  items: InventoryItem[]
}

export default function InventoryTab({ items }: InventoryTabProps) {
  const [selectedItem, setSelectedItem] = useState<InventoryItem | null>(null)
  const [giveQuantity, setGiveQuantity] = useState<number>(1)

  const handleUseItem = (item: InventoryItem) => {
    fetchNui('nuiCallback', {
      callback: 'player:useItem',
      itemName: item.name
    })
  }

  const handleGiveItem = (item: InventoryItem) => {
    if (giveQuantity > 0 && giveQuantity <= item.quantity) {
      fetchNui('nuiCallback', {
        callback: 'player:giveItem',
        itemName: item.name,
        quantity: giveQuantity
      })
      setSelectedItem(null)
      setGiveQuantity(1)
    }
  }

  if (items.length === 0) {
    return (
      <div className="empty-state">
        <Package size={48} />
        <h3>Inventaire vide</h3>
        <p>Vous n'avez aucun objet dans votre inventaire</p>
      </div>
    )
  }

  return (
    <div className="inventory-tab">
      <div className="inventory-grid">
        {items.map((item, index) => (
          <div 
            key={index} 
            className={`inventory-item ${selectedItem?.name === item.name ? 'selected' : ''}`}
            onClick={() => setSelectedItem(item)}
          >
            <div className="item-icon">{item.icon}</div>
            <div className="item-info">
              <div className="item-name">{item.name}</div>
              <div className="item-quantity">x{item.quantity}</div>
            </div>
          </div>
        ))}
      </div>

      {selectedItem && (
        <div className="item-actions">
          <h3>{selectedItem.icon} {selectedItem.name}</h3>
          <p className="item-quantity-text">Quantité disponible: {selectedItem.quantity}</p>
          
          <div className="action-buttons">
            <button 
              className="action-btn use"
              onClick={() => handleUseItem(selectedItem)}
            >
              <Play size={18} />
              Utiliser
            </button>

            <div className="give-section">
              <input 
                type="number" 
                min="1" 
                max={selectedItem.quantity}
                value={giveQuantity}
                onChange={(e) => setGiveQuantity(parseInt(e.target.value) || 1)}
                className="give-input"
                placeholder="Quantité"
              />
              <button 
                className="action-btn give"
                onClick={() => handleGiveItem(selectedItem)}
              >
                <Send size={18} />
                Donner
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}