import { useState } from 'react'
import { X, User, Package, Shirt, FileText, Crosshair, ShoppingBag, Shield } from 'lucide-react'
import { fetchNui } from '../utils/fetchNui'

import InventoryTab from './PlayerMenu/InventoryTab'
import ClothesTab from './PlayerMenu/ClothesTab'
import BillsTab from './PlayerMenu/BillsTab'
import WeaponsTab from './PlayerMenu/WeaponsTab'
import ShopTab from './PlayerMenu/ShopTab'
import InfoTab from './PlayerMenu/InfoTab'
import AdminTab from './PlayerMenu/AdminTab'

interface PlayerMenuProps {
  data: {
    playerInfo: {
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
    inventory: Array<{
      name: string
      quantity: number
      type: string
      icon: string
    }>
    weapons: Array<{
      name: string
      ammo: number
      icon: string
    }>
    clothes: Array<{
      name: string
      components: number
    }>
    bills: Array<{
      company: string
      author: string
      date: string
      amount: number
    }>
    vehicles: Array<{
      name: string
      model: string
      price: number
    }>
    isAdmin: boolean
    isDead?: boolean
    players: Array<any>
  }
  onClose: () => void
}

type TabType = 'info' | 'inventory' | 'weapons' | 'clothes' | 'bills' | 'shop' | 'admin'

export default function PlayerMenu({ data, onClose }: PlayerMenuProps) {
  const [activeTab, setActiveTab] = useState<TabType>('info')

  const formatMoney = (value: number) => {
    return `$${value.toLocaleString('en-US')}`
  }

  const tabs = [
    { id: 'info' as TabType, label: 'Informations', icon: User },
    { id: 'inventory' as TabType, label: 'Inventaire', icon: Package },
    { id: 'weapons' as TabType, label: 'Armes', icon: Crosshair },
    { id: 'clothes' as TabType, label: 'Vêtements', icon: Shirt },
    { id: 'bills' as TabType, label: 'Factures', icon: FileText },
    { id: 'shop' as TabType, label: 'Boutique', icon: ShoppingBag },
  ]

  if (data.isAdmin) {
    tabs.push({ id: 'admin' as TabType, label: 'Admin', icon: Shield })
  }

  const handleClose = () => {
    fetchNui('nuiCallback', {
      callback: 'player:onClose'
    })
    onClose()
  }

  return (
    <div className="player-menu-container">
      <div className="player-menu-card">
        {/* Header */}
        <div className="player-menu-header">
          <div className="header-content">
            <div className="player-avatar">
              <User size={32} />
            </div>
            <div className="player-info-header">
              <h1 className="player-name">{data.playerInfo.firstname} {data.playerInfo.lastname}</h1>
              <p className="player-subtitle">
                {data.playerInfo.rank === 'staff' && <Shield size={14} style={{display: 'inline', marginRight: '4px'}} />}
                {data.playerInfo.rank === 'staff' ? 'Staff' : 'Citoyen'}
              </p>
            </div>
          </div>
          
          {/* Money Display */}
          <div className="money-display">
            <div className="money-item bank">
              <span className="money-icon">💳</span>
              <div>
                <div className="money-label">Banque</div>
                <div className="money-amount">{formatMoney(data.playerInfo.money)}</div>
              </div>
            </div>
            <div className="money-item cash">
              <span className="money-icon">💵</span>
              <div>
                <div className="money-label">Liquide</div>
                <div className="money-amount">{formatMoney(data.playerInfo.cash)}</div>
              </div>
            </div>
            <div className="money-item bitcoin">
              <span className="money-icon">₿</span>
              <div>
                <div className="money-label">Bitcoin</div>
                <div className="money-amount">{data.playerInfo.bitcoin} BTC</div>
              </div>
            </div>
          </div>

          <button onClick={handleClose} className="close-button">
            <X size={20} />
          </button>
        </div>

        {/* Tabs Navigation */}
        <div className="tabs-navigation">
          {tabs.map(tab => {
            const Icon = tab.icon
            return (
              <button
                key={tab.id}
                className={`tab-button ${activeTab === tab.id ? 'active' : ''}`}
                onClick={() => setActiveTab(tab.id)}
              >
                <Icon size={18} />
                <span>{tab.label}</span>
              </button>
            )
          })}
        </div>

        {/* Tab Content */}
        <div className="tab-content">
          {activeTab === 'info' && <InfoTab data={data.playerInfo} isDead={data.isDead} />}
          {activeTab === 'inventory' && <InventoryTab items={data.inventory} />}
          {activeTab === 'weapons' && <WeaponsTab weapons={data.weapons} />}
          {activeTab === 'clothes' && <ClothesTab clothes={data.clothes} />}
          {activeTab === 'bills' && <BillsTab bills={data.bills} playerMoney={data.playerInfo.money} />}
          {activeTab === 'shop' && <ShopTab vehicles={data.vehicles} bitcoin={data.playerInfo.bitcoin} />}
          {activeTab === 'admin' && data.isAdmin && <AdminTab players={data.players} />}
        </div>
      </div>
    </div>
  )
}