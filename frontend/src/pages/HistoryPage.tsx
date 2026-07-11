import { Button, Card, Empty, List, Popconfirm, Space, Tag, Typography } from 'antd';
import { DeleteOutlined, RightOutlined } from '@ant-design/icons';
import { Link } from 'react-router-dom';
import dayjs from 'dayjs';
import { useDeleteHistory, useHistory } from '../hooks/useTranscripts';

/** All past analyses (saved in this browser), newest first. */
export function HistoryPage() {
  const history = useHistory();
  const { removeItem, clearAll } = useDeleteHistory();
  const items = history.data ?? [];

  return (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      <Space style={{ width: '100%', justifyContent: 'space-between' }}>
        <div>
          <Typography.Title level={3} style={{ marginBottom: 4 }}>
            History
          </Typography.Title>
          <Typography.Text type="secondary">
            Analyses saved in this browser ({items.length}).
          </Typography.Text>
        </div>
        {items.length > 0 && (
          <Popconfirm title="Delete ALL saved analyses?" onConfirm={clearAll}>
            <Button danger icon={<DeleteOutlined />}>
              Clear all
            </Button>
          </Popconfirm>
        )}
      </Space>

      <Card>
        {items.length === 0 ? (
          <Empty description="No analyses yet — run one on the New Transcription page." />
        ) : (
          <List
            itemLayout="horizontal"
            dataSource={items}
            renderItem={(item) => {
              const snippet =
                item.request.transcriptText.length > 120
                  ? `${item.request.transcriptText.slice(0, 120)}…`
                  : item.request.transcriptText;
              return (
                <List.Item
                  actions={[
                    <Link key="open" to={`/transcription/${item.id}`}>
                      <Button type="link" icon={<RightOutlined />}>
                        Open
                      </Button>
                    </Link>,
                    <Popconfirm
                      key="delete"
                      title="Delete this analysis?"
                      onConfirm={() => removeItem(item.id)}
                    >
                      <Button type="link" danger icon={<DeleteOutlined />} />
                    </Popconfirm>,
                  ]}
                >
                  <List.Item.Meta
                    title={
                      <Space size="small">
                        <Link to={`/transcription/${item.id}`}>
                          {item.response.extractedAttributes.name ?? 'Unknown caller'}
                        </Link>
                        <Tag color={item.request.language === 'hy' ? 'volcano' : 'blue'}>
                          {item.request.language === 'hy' ? 'Armenian' : 'English'}
                        </Tag>
                      </Space>
                    }
                    description={
                      <Space direction="vertical" size={2}>
                        <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                          {dayjs(item.createdAt).format('MMM D, YYYY HH:mm')} ·{' '}
                          {item.response.conversation.length} turns
                        </Typography.Text>
                        <Typography.Text type="secondary">{snippet}</Typography.Text>
                      </Space>
                    }
                  />
                </List.Item>
              );
            }}
          />
        )}
      </Card>
    </Space>
  );
}
