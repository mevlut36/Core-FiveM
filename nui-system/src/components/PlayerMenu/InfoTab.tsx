// InfoTab.tsx
import { MapPin, Zap, Heart } from 'lucide-react'
import { fetchNui } from '../../utils/fetchNui'

interface InfoTabProps {
  data: {
    firstname: string
    lastname: string
    birth: string
    money: number
    cash: number
    bitcoin: number
    rank: string
    job: string
    jobRank: number
  }
  isDead?: boolean
}

export default function InfoTab({ data, isDead }: InfoTabProps) {
  const isStaff = data.rank === 'staff'

  const handleShowCoordinates = () => {
    fetchNui('nuiCallback', {
      callback: 'player:showCoordinates'
    })
  }

  const handleReviveSelf = () => {
    fetchNui('nuiCallback', {
      callback: 'player:reviveSelf'
    })
  }

  const handleQuickNoclip = () => {
    fetchNui('nuiCallback', {
      callback: 'player:quickNoclip'
    })
  }

  return (
    <div style={{ padding: '24px' }}>
      {isStaff && (
        <div style={{ 
          marginBottom: '32px',
          background: 'rgba(139, 92, 246, 0.1)',
          border: '1px solid rgba(139, 92, 246, 0.3)',
          borderRadius: '12px',
          padding: '20px'
        }}>
          <h3 style={{ 
            color: '#c4b5fd',
            fontSize: '16px',
            fontWeight: '600',
            marginBottom: '16px',
            display: 'flex',
            alignItems: 'center',
            gap: '8px'
          }}>
            <Zap size={18} />
            Actions Rapides Staff
          </h3>
          
          <div style={{ 
            display: 'grid',
            gridTemplateColumns: isDead ? '1fr 1fr' : '1fr 1fr 1fr',
            gap: '12px'
          }}>
            {isDead && (
              <button 
                onClick={handleReviveSelf}
                style={{
                  background: 'rgba(34, 197, 94, 0.2)',
                  border: '1px solid rgba(34, 197, 94, 0.3)',
                  borderRadius: '8px',
                  padding: '12px 16px',
                  color: '#86efac',
                  cursor: 'pointer',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  gap: '8px',
                  fontSize: '14px',
                  fontWeight: '600',
                  transition: 'all 0.2s'
                }}
                onMouseOver={(e) => {
                  e.currentTarget.style.background = 'rgba(34, 197, 94, 0.3)'
                  e.currentTarget.style.transform = 'translateY(-2px)'
                }}
                onMouseOut={(e) => {
                  e.currentTarget.style.background = 'rgba(34, 197, 94, 0.2)'
                  e.currentTarget.style.transform = 'translateY(0)'
                }}
              >
                <Heart size={16} />
                Se Réanimer
              </button>
            )}
            
            <button 
              onClick={handleQuickNoclip}
              style={{
                background: 'rgba(139, 92, 246, 0.2)',
                border: '1px solid rgba(139, 92, 246, 0.3)',
                borderRadius: '8px',
                padding: '12px 16px',
                color: '#c4b5fd',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                gap: '8px',
                fontSize: '14px',
                fontWeight: '600',
                transition: 'all 0.2s'
              }}
              onMouseOver={(e) => {
                e.currentTarget.style.background = 'rgba(139, 92, 246, 0.3)'
                e.currentTarget.style.transform = 'translateY(-2px)'
              }}
              onMouseOut={(e) => {
                e.currentTarget.style.background = 'rgba(139, 92, 246, 0.2)'
                e.currentTarget.style.transform = 'translateY(0)'
              }}
            >
              <Zap size={16} />
              NoClip
            </button>
            
            <button 
              onClick={handleShowCoordinates}
              style={{
                background: 'rgba(59, 130, 246, 0.2)',
                border: '1px solid rgba(59, 130, 246, 0.3)',
                borderRadius: '8px',
                padding: '12px 16px',
                color: '#93c5fd',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                gap: '8px',
                fontSize: '14px',
                fontWeight: '600',
                transition: 'all 0.2s'
              }}
              onMouseOver={(e) => {
                e.currentTarget.style.background = 'rgba(59, 130, 246, 0.3)'
                e.currentTarget.style.transform = 'translateY(-2px)'
              }}
              onMouseOut={(e) => {
                e.currentTarget.style.background = 'rgba(59, 130, 246, 0.2)'
                e.currentTarget.style.transform = 'translateY(0)'
              }}
            >
              <MapPin size={16} />
              Coordonnées
            </button>
          </div>
        </div>
      )}

      {!isStaff && (
        <div style={{ marginBottom: '24px' }}>
          <button 
            onClick={handleShowCoordinates}
            style={{
              background: 'rgba(59, 130, 246, 0.2)',
              border: '1px solid rgba(59, 130, 246, 0.3)',
              borderRadius: '8px',
              padding: '12px 16px',
              color: '#93c5fd',
              cursor: 'pointer',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: '8px',
              fontSize: '14px',
              fontWeight: '600',
              width: '100%',
              transition: 'all 0.2s'
            }}
            onMouseOver={(e) => {
              e.currentTarget.style.background = 'rgba(59, 130, 246, 0.3)'
              e.currentTarget.style.transform = 'translateY(-2px)'
            }}
            onMouseOut={(e) => {
              e.currentTarget.style.background = 'rgba(59, 130, 246, 0.2)'
              e.currentTarget.style.transform = 'translateY(0)'
            }}
          >
            <MapPin size={16} />
            Voir les Coordonnées
          </button>
        </div>
      )}

      <div style={{ 
        background: 'rgba(255, 255, 255, 0.05)',
        border: '1px solid rgba(255, 255, 255, 0.1)',
        borderRadius: '12px',
        padding: '24px'
      }}>
        <h3 style={{ 
          color: 'white',
          fontSize: '18px',
          fontWeight: '600',
          marginBottom: '20px'
        }}>
          Informations Personnelles
        </h3>

        <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <span style={{ color: 'rgba(255, 255, 255, 0.6)', fontSize: '14px' }}>
              Nom complet
            </span>
            <span style={{ color: 'white', fontSize: '14px', fontWeight: '600' }}>
              {data.firstname} {data.lastname}
            </span>
          </div>

          <div style={{ 
            height: '1px',
            background: 'rgba(255, 255, 255, 0.1)'
          }} />

          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <span style={{ color: 'rgba(255, 255, 255, 0.6)', fontSize: '14px' }}>
              Date de naissance
            </span>
            <span style={{ color: 'white', fontSize: '14px', fontWeight: '600' }}>
              {data.birth}
            </span>
          </div>

          <div style={{ 
            height: '1px',
            background: 'rgba(255, 255, 255, 0.1)'
          }} />

          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <span style={{ color: 'rgba(255, 255, 255, 0.6)', fontSize: '14px' }}>
              Emploi
            </span>
            <span style={{ color: 'white', fontSize: '14px', fontWeight: '600' }}>
              {data.job} (Rang {data.jobRank})
            </span>
          </div>

          <div style={{ 
            height: '1px',
            background: 'rgba(255, 255, 255, 0.1)'
          }} />

          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <span style={{ color: 'rgba(255, 255, 255, 0.6)', fontSize: '14px' }}>
              Statut
            </span>
            <span style={{ 
              color: isStaff ? '#c4b5fd' : 'white',
              fontSize: '14px',
              fontWeight: '600'
            }}>
              {isStaff ? '🛡️ Staff' : 'Citoyen'}
            </span>
          </div>
        </div>
      </div>
    </div>
  )
}