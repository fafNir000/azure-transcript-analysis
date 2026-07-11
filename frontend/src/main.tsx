import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ConfigProvider } from 'antd';
import App from './App';
import './index.css';

// One React Query client for the whole app (caching + request state).
const queryClient = new QueryClient();

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <ConfigProvider
          theme={{
            token: {
              colorPrimary: '#2f54eb',
              borderRadius: 8,
              fontFamily:
                "-apple-system, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, 'Noto Sans Armenian', sans-serif",
            },
          }}
        >
          <App />
        </ConfigProvider>
      </BrowserRouter>
    </QueryClientProvider>
  </StrictMode>,
);
