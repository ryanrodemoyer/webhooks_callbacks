import Webhooks from 'components/webhooks'
import HomePage from 'components/home-page'

export const routes = [
  { name: 'home', path: '/', component: HomePage, display: 'Home', icon: 'home' },
  { name: 'webhooks', path: '/webhooks', component: Webhooks, display: 'Webhooks', icon: 'list' }
]
