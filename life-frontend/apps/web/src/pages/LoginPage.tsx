import { useNavigate } from 'react-router-dom';
import { session, translate } from '@life/core';
import { ModulButton } from '@life/ui';

export default function LoginPage() {
  const navigate = useNavigate();

  return (
    <main className="home">
      <h1>{translate('user.login')}</h1>
      <ModulButton
        label={translate('user.loginDemo')}
        onPress={() => {
          session.signIn({ id: 'demo', name: 'Usuario Demo' });
          navigate('/');
        }}
      />
    </main>
  );
}
