import { useState, useEffect } from 'react'
import { X } from 'lucide-react'

interface InputModalProps {
  isOpen: boolean
  title: string
  placeholder?: string
  maxLength?: number
  onConfirm: (value: string) => void
  onCancel: () => void
}

export default function InputModal({ 
  isOpen, 
  title, 
  placeholder = '', 
  maxLength = 100,
  onConfirm, 
  onCancel 
}: InputModalProps) {
  const [value, setValue] = useState('')

  useEffect(() => {
    if (!isOpen) {
      setValue('')
    }
  }, [isOpen])

  useEffect(() => {
    const handleKeyPress = (e: KeyboardEvent) => {
      if (!isOpen) return
      
      if (e.key === 'Enter' && value.trim()) {
        handleConfirm()
      } else if (e.key === 'Escape') {
        onCancel()
      }
    }

    window.addEventListener('keydown', handleKeyPress)
    return () => window.removeEventListener('keydown', handleKeyPress)
  }, [isOpen, value])

  const handleConfirm = () => {
    if (value.trim()) {
      onConfirm(value)
      setValue('')
    }
  }

  if (!isOpen) return null

  return (
    <div style={{
      position: 'fixed',
      top: 0,
      left: 0,
      right: 0,
      bottom: 0,
      background: 'rgba(0, 0, 0, 0.7)',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      zIndex: 9999,
      backdropFilter: 'blur(4px)'
    }}>
      <div style={{
        background: 'linear-gradient(145deg, #1a1d29 0%, #23262f 100%)',
        borderRadius: '16px',
        width: '90%',
        maxWidth: '500px',
        border: '1px solid rgba(255, 255, 255, 0.1)',
        overflow: 'hidden',
        animation: 'fadeIn 0.2s ease'
      }}>
        {/* Header */}
        <div style={{
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          padding: '20px 24px',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center'
        }}>
          <h3 style={{
            color: 'white',
            fontSize: '18px',
            fontWeight: '700',
            margin: 0
          }}>
            {title}
          </h3>
          <button
            onClick={onCancel}
            style={{
              background: 'rgba(255, 255, 255, 0.2)',
              border: 'none',
              borderRadius: '8px',
              width: '32px',
              height: '32px',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              cursor: 'pointer',
              color: 'white',
              transition: 'background 0.2s'
            }}
            onMouseEnter={(e) => e.currentTarget.style.background = 'rgba(255, 255, 255, 0.3)'}
            onMouseLeave={(e) => e.currentTarget.style.background = 'rgba(255, 255, 255, 0.2)'}
          >
            <X size={18} />
          </button>
        </div>

        {/* Body */}
        <div style={{ padding: '24px' }}>
          <input
            type="text"
            value={value}
            onChange={(e) => setValue(e.target.value)}
            placeholder={placeholder}
            maxLength={maxLength}
            autoFocus
            style={{
              width: '100%',
              background: 'rgba(255, 255, 255, 0.05)',
              border: '1px solid rgba(255, 255, 255, 0.1)',
              borderRadius: '12px',
              padding: '16px',
              color: 'white',
              fontSize: '16px',
              outline: 'none',
              transition: 'all 0.2s',
              marginBottom: '20px'
            }}
            onFocus={(e) => {
              e.target.style.background = 'rgba(255, 255, 255, 0.08)'
              e.target.style.borderColor = '#667eea'
            }}
            onBlur={(e) => {
              e.target.style.background = 'rgba(255, 255, 255, 0.05)'
              e.target.style.borderColor = 'rgba(255, 255, 255, 0.1)'
            }}
          />

          <div style={{
            display: 'flex',
            gap: '12px'
          }}>
            <button
              onClick={onCancel}
              style={{
                flex: 1,
                padding: '12px',
                border: 'none',
                borderRadius: '8px',
                background: 'rgba(255, 255, 255, 0.1)',
                color: 'rgba(255, 255, 255, 0.8)',
                fontSize: '14px',
                fontWeight: '600',
                cursor: 'pointer',
                transition: 'all 0.2s'
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.background = 'rgba(255, 255, 255, 0.15)'
                e.currentTarget.style.transform = 'translateY(-2px)'
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.background = 'rgba(255, 255, 255, 0.1)'
                e.currentTarget.style.transform = 'translateY(0)'
              }}
            >
              Annuler
            </button>
            <button
              onClick={handleConfirm}
              disabled={!value.trim()}
              style={{
                flex: 1,
                padding: '12px',
                border: 'none',
                borderRadius: '8px',
                background: value.trim() 
                  ? 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' 
                  : 'rgba(255, 255, 255, 0.05)',
                color: value.trim() ? 'white' : 'rgba(255, 255, 255, 0.3)',
                fontSize: '14px',
                fontWeight: '600',
                cursor: value.trim() ? 'pointer' : 'not-allowed',
                transition: 'all 0.2s'
              }}
              onMouseEnter={(e) => {
                if (value.trim()) {
                  e.currentTarget.style.transform = 'translateY(-2px)'
                  e.currentTarget.style.boxShadow = '0 8px 20px rgba(102, 126, 234, 0.4)'
                }
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.transform = 'translateY(0)'
                e.currentTarget.style.boxShadow = 'none'
              }}
            >
              Confirmer
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}