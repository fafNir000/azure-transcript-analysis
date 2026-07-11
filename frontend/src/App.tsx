import { Navigate, Route, Routes } from 'react-router-dom';
import { AppLayout } from './components/AppLayout';
import { NewTranscriptionPage } from './pages/NewTranscriptionPage';
import { HistoryPage } from './pages/HistoryPage';
import { TranscriptionDetailsPage } from './pages/TranscriptionDetailsPage';

export default function App() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route path="/" element={<NewTranscriptionPage />} />
        <Route path="/history" element={<HistoryPage />} />
        <Route path="/transcription/:id" element={<TranscriptionDetailsPage />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}
