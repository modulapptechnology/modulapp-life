import { useNavigate } from 'react-router-dom';
import { translate } from '@life/core';
import { ModulEmptyState } from '@life/ui';

export default function UnavailablePage() {
  const navigate = useNavigate();

  return (
    <ModulEmptyState
      title={translate('module.unavailable.title')}
      description={translate('module.unavailable.description')}
      actionLabel={translate('module.unavailable.back')}
      onAction={() => navigate('/')}
    />
  );
}
