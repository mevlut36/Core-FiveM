import { useState } from 'react'
import { Car, Crown, Sparkles } from 'lucide-react'
import { fetchNui } from '../../utils/fetchNui'

interface Vehicle {
  name: string
  model: string
  price: number
}

interface ShopTabProps {
  vehicles: Vehicle[]
  bitcoin: number
}

type ShopCategory = 'vip' | 'vehicles'

export default function ShopTab({ vehicles, bitcoin }: ShopTabProps) {
  const [activeCategory, setActiveCategory] = useState<ShopCategory>('vip')

  const vipPlans = [
    {
      name: 'VIP',
      price: 500,
      color: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
      features: [
        'Accès aux zones VIP',
        '+10% d\'argent sur les missions',
        'Véhicules exclusifs',
        'Support prioritaire'
      ]
    },
    {
      name: 'VIP+',
      price: 700,
      color: 'linear-gradient(135deg, #ffd89b 0%, #19547b 100%)',
      features: [
        'Tous les avantages VIP',
        '+20% d\'argent sur les missions',
        'Maisons exclusives',
        'Tag personnalisé'
      ]
    },
    {
      name: 'MVP',
      price: 1000,
      color: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      features: [
        'Tous les avantages VIP+',
        '+30% d\'argent sur les missions',
        'Accès anticipé aux nouveautés',
        'Badge exclusif'
      ]
    }
  ]

  const handleBuyVIP = (vipType: string, price: number) => {
    fetchNui('nuiCallback', {
      callback: 'player:buyVIP',
      vipType,
      price
    })
  }

  const handleBuyVehicle = (vehicleName: string, price: number) => {
    fetchNui('nuiCallback', {
      callback: 'player:buyImportVehicle',
      vehicleName,
      price
    })
  }

  return (
    <div className="shop-tab">
      {/* Categories */}
      <div className="shop-categories">
        <button 
          className={`category-btn ${activeCategory === 'vip' ? 'active' : ''}`}
          onClick={() => setActiveCategory('vip')}
        >
          <Crown size={18} />
          VIP & Abonnements
        </button>
        <button 
          className={`category-btn ${activeCategory === 'vehicles' ? 'active' : ''}`}
          onClick={() => setActiveCategory('vehicles')}
        >
          <Car size={18} />
          Véhicules d'Import
        </button>
      </div>

      {/* Bitcoin Display */}
      <div style={{
        background: 'rgba(255, 193, 7, 0.1)',
        border: '1px solid rgba(255, 193, 7, 0.3)',
        borderRadius: '12px',
        padding: '16px',
        marginBottom: '24px',
        display: 'flex',
        alignItems: 'center',
        gap: '12px'
      }}>
        <span style={{ fontSize: '24px' }}>₿</span>
        <div>
          <div style={{ color: 'rgba(255, 255, 255, 0.7)', fontSize: '12px' }}>
            Votre solde Bitcoin
          </div>
          <div style={{ color: '#ffc107', fontSize: '20px', fontWeight: '700' }}>
            {bitcoin} BTC
          </div>
        </div>
      </div>

      {/* VIP Plans */}
      {activeCategory === 'vip' && (
        <div className="shop-grid">
          {vipPlans.map((plan, index) => (
            <div key={index} className="shop-item vip-item">
              <div style={{
                background: plan.color,
                borderRadius: '12px',
                padding: '24px',
                marginBottom: '20px',
                textAlign: 'center'
              }}>
                <Crown size={40} style={{ marginBottom: '12px' }} />
                <h3 style={{ 
                  color: 'white', 
                  fontSize: '24px', 
                  fontWeight: '700', 
                  margin: '0 0 8px 0' 
                }}>
                  {plan.name}
                </h3>
                <div style={{ 
                  color: 'rgba(255, 255, 255, 0.9)', 
                  fontSize: '32px', 
                  fontWeight: '700' 
                }}>
                  {plan.price} BTC
                </div>
                <div style={{ 
                  color: 'rgba(255, 255, 255, 0.8)', 
                  fontSize: '13px',
                  marginTop: '4px'
                }}>
                  par mois
                </div>
              </div>

              <div style={{ marginBottom: '20px' }}>
                {plan.features.map((feature, i) => (
                  <div key={i} style={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: '8px',
                    padding: '8px 0',
                    color: 'rgba(255, 255, 255, 0.8)',
                    fontSize: '14px'
                  }}>
                    <Sparkles size={16} style={{ color: '#ffc107', flexShrink: 0 }} />
                    {feature}
                  </div>
                ))}
              </div>

              <button 
                className="shop-item-btn"
                onClick={() => handleBuyVIP(plan.name, plan.price)}
                disabled={bitcoin < plan.price}
                style={{ background: plan.color }}
              >
                {bitcoin >= plan.price ? 'Acheter' : 'Bitcoins insuffisants'}
              </button>
            </div>
          ))}
        </div>
      )}

      {/* Import Vehicles */}
      {activeCategory === 'vehicles' && (
        <div className="shop-grid">
          {vehicles.map((vehicle, index) => (
            <div key={index} className="shop-item">
              <div className="shop-item-header">
                <div>
                  <h3 className="shop-item-title">{vehicle.name}</h3>
                  <p style={{ 
                    color: 'rgba(255, 255, 255, 0.6)', 
                    fontSize: '13px',
                    margin: '4px 0 0 0'
                  }}>
                    {vehicle.model}
                  </p>
                </div>
                <div className="shop-item-price">
                  {vehicle.price} BTC
                </div>
              </div>

              <div style={{
                background: 'rgba(255, 255, 255, 0.03)',
                borderRadius: '8px',
                padding: '16px',
                marginBottom: '16px'
              }}>
                <Car size={48} style={{ color: 'rgba(255, 255, 255, 0.3)' }} />
              </div>

              <button 
                className="shop-item-btn"
                onClick={() => handleBuyVehicle(vehicle.model, vehicle.price)}
                disabled={bitcoin < vehicle.price}
              >
                {bitcoin >= vehicle.price ? 'Acheter' : 'Bitcoins insuffisants'}
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}