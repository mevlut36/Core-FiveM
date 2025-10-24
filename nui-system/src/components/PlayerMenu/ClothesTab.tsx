// ClothesTab.tsx
import { Shirt } from 'lucide-react'
import { fetchNui } from '../../utils/fetchNui'

interface ClothesItem {
  name: string
  components: number
}

interface ClothesTabProps {
  clothes: ClothesItem[]
}

export default function ClothesTab({ clothes }: ClothesTabProps) {
  const handleToggleClothes = (clothesName: string, action: 'wear' | 'remove') => {
    fetchNui('nuiCallback', {
      callback: 'player:toggleClothes',
      clothesName,
      action
    })
  }

  if (clothes.length === 0) {
    return (
      <div className="empty-state">
        <Shirt size={48} />
        <h3>Aucun vêtement</h3>
        <p>Vous n'avez aucun ensemble de vêtements sauvegardé</p>
      </div>
    )
  }

  return (
    <div className="clothes-grid">
      {clothes.map((item, index) => (
        <div key={index} className="clothes-card">
          <div className="card-header">
            <span className="card-icon">👔</span>
            <div>
              <h3 className="card-title">{item.name}</h3>
              <p className="card-subtitle">{item.components} composants</p>
            </div>
          </div>
          <div className="card-actions">
            <button 
              className="card-btn primary"
              onClick={() => handleToggleClothes(item.name, 'wear')}
            >
              Porter
            </button>
            <button 
              className="card-btn secondary"
              onClick={() => handleToggleClothes(item.name, 'remove')}
            >
              Enlever
            </button>
          </div>
        </div>
      ))}
    </div>
  )
}